using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;

using Google.Cloud.Translate.V3;
using Google.LongRunning;

namespace GTBench
{
    using Google.Protobuf;
    using static GTBench.Helpers;

    /// <summary>
    /// Manages the list of glossaries.
    /// </summary>
    /// <remarks>
    /// This class is part of WPF UI.
    /// All methods (including constructors) should only be executed by the UI thread.
    /// </remarks>
    public class GlossaryManager
    {
        private static readonly Properties.Settings Settings = Properties.Settings.Default;

        private readonly ObservableCollection<GlossaryInfo> List
            = new ObservableCollection<GlossaryInfo>();

        private readonly DispatcherTimer Timer;

        public GlossaryManager()
        {
            Timer = new DispatcherTimer();
            Timer.Stop();
            Timer.Tick += Timer_Tick;
        }

        private async void Timer_Tick(object sender, EventArgs args)
        {
            // Avoid re-entry to this event handler.
            Timer.Stop();

            bool completed = false;
            foreach (var info in List.Where(i => (i.Status & GlossaryStatus.RunningFlag) != 0).ToArray())
            {
                try
                {
                    var client = await GetTranslationServiceClientAsync();
                    switch (info.Status)
                    {
                        case GlossaryStatus.Creating:
                            completed |= HandlePoll(info,
                                await client.PollOnceCreateGlossaryAsync(info.OperationName));
                            break;
                        case GlossaryStatus.Deleting:
                            completed |= HandlePoll(info,
                                await client.PollOnceDeleteGlossaryAsync(info.OperationName));
                            break;
                        default:
                            throw new ApplicationException($"Unknown status {info.Status}");
                    }
                }
#pragma warning disable 0168
                catch (Exception exception)
                {
                    // I'm not very sure what we should do.
                    // For now, just ignore the exception to keep polling,
                    // hoping that we will not get the same error next time...
                }
#pragma warning restore 0168
            }

            if (completed)
            {
                // One or more long-running operations has been completed.
                // Remove any faint info perhaps from the completed deletion.
                for (int p = List.Count - 1; p >= 0; --p)
                {
                    if (List[p].Status == GlossaryStatus.Faint) List.RemoveAt(p);
                }

                // Anyway persists the remaining long-running operations.
                PersistRunningOperations();
            }

            // Update the timer
            if (List.All(i => (i.Status & GlossaryStatus.RunningFlag) == 0))
            {
                // No more long-running operations.  Stop polling.
                // (We have stopped the timer already, so do nothing here.)
            }
            else if (completed)
            {
                // Some operations were completed but there are more.
                // Keep the same interval for the next polling.
                Timer.Start();
            }
            else
            {
                // No long-running operation was completed in this poll.
                // make the polling interval longer (like exponential backoff.)
                var seconds = Timer.Interval.TotalSeconds;
                seconds *= 1.5;
                seconds = Math.Max(seconds, Settings.MinimumOperationPollInterval.TotalSeconds);
                seconds = Math.Min(seconds, Settings.MaximumOperationPollInterval.TotalSeconds);
                Timer.Interval = TimeSpan.FromSeconds(seconds);
                Timer.Start();
            }
        }

        private bool HandlePoll<TResponse, TMetadata>(GlossaryInfo info, Operation<TResponse, TMetadata> operation)
            where TResponse : class, IMessage<TResponse>, new()
            where TMetadata : class, IMessage<TMetadata>, new()
        {
            if (operation.IsFaulted)
            {
                info.Status = GlossaryStatus.Error;
                info.AlertMessage = operation.Exception?.Message ?? "(Faulted)";
                return true;
            }
            else if (operation.IsCompleted)
            {
                if ((object)operation.Result is Glossary glossary)
                {
                    info.UpdateFrom(glossary);
                }
                else if ((object)operation.Result is DeleteGlossaryResponse delete)
                {
                    info.Status = GlossaryStatus.Faint;
                }
                else
                {
                    throw new ApplicationException($"Unknown TResponse type {typeof(TResponse)}");
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartPolling()
        {
            Timer.Interval = TimeSpan.Zero;
            Timer.Start();
        }

        public void StopPolling()
        {
            Timer.Stop();
        }

        /// <summary>
        /// Gets an object suitable for a binding source 
        /// that represents the collection of <see cref="GlossaryInfo"/>.
        /// </summary>
        public ICollection<GlossaryInfo> DataSource => List;

        public async Task RefreshAsync()
        {
            StopPolling();
            List.Clear();

            // Add all known glossaries from the cloud.
            var client = await GetTranslationServiceClientAsync();
            var response = client.ListGlossariesAsync(GetLocationName());
            await response.ForEachAsync(glossary =>
            {
                var info = new GlossaryInfo(glossary);
                var id = glossary.GlossaryName.GlossaryId;
                if (IndexOf(id) is int p && p >= 0)
                {
                    List[p] = info;
                }
                else
                {
                    List.Add(info);
                }
            });

            // Add running operations.
            RestoreRunningOperations();

            StartPolling();
        }

        public async Task CreateAsync(string input_uri)
        {
            // GCT doesn't allow duplicate glossary id,
            // and we can't because we use it as a _primary key_ in this class.
            // However, if it is an error info, we can simply remove it.
            var id = Settings.GlossaryID;
            if (IndexOf(id) is int p && p >= 0 && List[p].Status == GlossaryStatus.Error)
            {
                List.RemoveAt(p);
            }
            else if (p >= 0)
            {
                throw new InvalidOperationException($"Existing glossary ID {id}");
            }

            var glossary = new Glossary
            {
                InputConfig = new GlossaryInputConfig
                {
                    GcsSource = new GcsSource { InputUri = input_uri },
                },
                LanguagePair = new Glossary.Types.LanguageCodePair
                {
                    SourceLanguageCode = Settings.SourceLanguage,
                    TargetLanguageCode = Settings.TargetLanguage,
                },
                Name = GetGlossaryName(),
            };
            var client = await GetTranslationServiceClientAsync();
            var operation = await client.CreateGlossaryAsync(GetLocationName(), glossary);

            List.Add(new GlossaryInfo(glossary, GlossaryStatus.Creating)
            {
                OperationName = operation.Name
            });
            PersistRunningOperations();

            StartPolling();
        }

        public async Task DeleteAsync(string glossary_id)
        {
            int p = IndexOf(glossary_id);
            if (p <= 0)
            {
                throw new InvalidOperationException($"No such glossary ID {glossary_id}");
            }
            var info = List[p];

            var client = await GetTranslationServiceClientAsync();
            var operation = await client.DeleteGlossaryAsync(GetGlossaryName(glossary_id));

            info.Status = GlossaryStatus.Deleting;
            info.OperationName = operation.Name;
            PersistRunningOperations();

            StartPolling();
        }

        private int IndexOf(string glossary_id)
        {
            int p;
            for (p = List.Count - 1; p >= 0; --p)
            {
                if (List[p].GlossaryID == glossary_id) break;
            }
            return p;
        }

        private void PersistRunningOperations()
        {
            var xml = new XElement("LongRunning",
                List.Where(i => (i.Status & GlossaryStatus.RunningFlag) != 0)
                    .Select(i =>
                        new XElement("GlossaryOperation",
                            new XAttribute("type",
                                i.Status == GlossaryStatus.Creating ? "Create" :
                                i.Status == GlossaryStatus.Deleting ? "Delete" :
                                throw new ApplicationException($"invalid status {i.Status}")),
                            new XAttribute("id", i.GlossaryID),
                            new XAttribute("name", i.OperationName))));
            Settings.RunningGlossaryOperations = xml.ToString(SaveOptions.DisableFormatting);
        }

        private void RestoreRunningOperations()
        {
            var data = Settings.RunningGlossaryOperations;
            foreach (var operation in XElement.Parse(data).Elements("GlossaryOperation"))
            {
                var type = (string)operation.Attribute("type");
                var id = (string)operation.Attribute("id");
                var name = (string)operation.Attribute("name");
                if (string.IsNullOrWhiteSpace(type) ||
                    string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(name)) continue;

                GlossaryStatus status;
                switch (type)
                {
                    case "Create": status = GlossaryStatus.Creating; break;
                    case "Delete": status = GlossaryStatus.Deleting; break;
                    default:
                        continue;
                }
                int i = IndexOf(id);
                if (i >= 0)
                {
                    List[i].Status = status;
                    List[i].OperationName = name;
                }
                else
                {
                    List.Add(new GlossaryInfo(status, id, name));
                }
            }
        }
    }
}
