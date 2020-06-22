using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using Google.Protobuf.Collections;

namespace GTBench
{
    public static class Helpers
    {
        private class TranslationServiceClientTuple
        {
            public TranslationServiceClient Client;
            public string CredentialsPath;
        }

        private static readonly WeakReference<TranslationServiceClientTuple> TranslationServiceClientCache
            = new WeakReference<TranslationServiceClientTuple>(null);

        public static async Task<TranslationServiceClient> GetTranslationServiceClientAsync()
        {
            var path = Properties.Settings.Default.CredentialsPath;
            if (!TranslationServiceClientCache.TryGetTarget(out var tuple)
                || tuple.CredentialsPath != path)
            {
                var client = string.IsNullOrWhiteSpace(path)
                    ? await TranslationServiceClient.CreateAsync()
                    : await new TranslationServiceClientBuilder { CredentialsPath = path }.BuildAsync();
                tuple = new TranslationServiceClientTuple { Client = client, CredentialsPath = path };
                TranslationServiceClientCache.SetTarget(tuple);
            }
            return tuple.Client;
        }

        public static TranslateTextGlossaryConfig GetGlossaryConfig()
        {
            var settings = Properties.Settings.Default;
            if (string.IsNullOrWhiteSpace(settings.GlossaryID)) return null;
            return new TranslateTextGlossaryConfig
            {
                Glossary = GetGlossaryName(),
                IgnoreCase = settings.GlossaryIgnoresCase,
            };
        }

        public static void AddLabels(this MapField<string, string> map)
        {
            var settings = Properties.Settings.Default;
            if (!string.IsNullOrWhiteSpace(settings.LabelKey))
            {
                map.Add(settings.LabelKey, settings.LabelValue);
            }
        }

        public static LocationName GetLocationName()
        {
            var settings = Properties.Settings.Default;
            return new LocationName(settings.ProjectID, settings.LocationID);
        }

        public static string GetModelName()
        {
            var settings = Properties.Settings.Default;
            return (string.IsNullOrWhiteSpace(settings.ModelID))
                ? string.Empty
                : "projects/" + settings.ProjectID + "/locations/" + settings.LocationID + "/models/" + settings.ModelID;
        }

        /// <summary>
        /// Gets the cloud resource name for a glossary.
        /// </summary>
        /// <param name="glossary_id">Identifier of the glossary, an empty string, or null.  The default is null.</param>
        /// <returns>Cloud resource name.</returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="glossary_id"/> is a non-null empty string (<see cref="string.Empty"/>),
        /// this method returns an empty string, that <i>usually</i> means "no glossary"
        /// in Google Cloud Translation (Advanced) APIs.
        /// If <paramref name="glossary_id"/> is null, this method uses the glossary ID
        /// taken from <see cref="Properties.Settings.Default"/>.
        /// </para>
        /// <para>
        /// Glossary name includes project ID and location ID.
        /// They are taken from  <see cref="Properties.Settings.Default"/>.
        /// </para>
        /// </remarks>
        public static string GetGlossaryName(string glossary_id = null)
        {
            var settings = Properties.Settings.Default;
            var id = glossary_id ?? settings.GlossaryID;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;
            return "projects/" + settings.ProjectID + "/locations/" + settings.LocationID + "/glossaries/" + id;
        }
    }
}
