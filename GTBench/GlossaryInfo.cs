using Google.Cloud.Translate.V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBench
{
    public enum GlossaryStatus
    {
        None = 0,

        /// <summary>A flag to indicates that a long running operation exists.</summary>
        RunningFlag = 0x10,

        /// <summary>A flag to mark an actually non-existing glossary info.</summary>
        RemnantFlag = 0x20,

        /// <summary>A glossary is being created.</summary>
        Creating = 0x11,

        /// <summary>A glossary is being deleted.</summary>
        Deleting = 0x12,

        /// <summary>Lifecycle of a GlossaryInfo is finished and will be removed automatically.</summary>
        Faint = 0x21,

        /// <summary>An error occured with this glossary info.</summary>
        Error = 0x22,
    }

    public class GlossaryInfo : INotifyPropertyChanged
    {
        public GlossaryInfo(GlossaryStatus status, string glossary_id, string operation_name)
        {
            Status = status;
            GlossaryID = glossary_id;
            OperationName = operation_name;

            Type = "?";
            SourceLanguage = string.Empty;
            TargetLanguage = string.Empty;
            InputUri = string.Empty;
            _Entries = 0;
        }

        public GlossaryInfo(Glossary glossary, GlossaryStatus status = GlossaryStatus.None)
        {
            UpdateFrom(glossary, status);
        }

        public void UpdateFrom(Glossary glossary, GlossaryStatus status = GlossaryStatus.None)
        {
            Status = status;
            GlossaryID = glossary.GlossaryName.GlossaryId;
            switch (glossary.LanguagesCase)
            {
                case Glossary.LanguagesOneofCase.LanguageCodesSet:
                    Type = "M";
                    SourceLanguage = string.Join(", ", glossary.LanguageCodesSet.LanguageCodes);
                    TargetLanguage = SourceLanguage;
                    break;
                case Glossary.LanguagesOneofCase.LanguagePair:
                    Type = "U";
                    SourceLanguage = glossary.LanguagePair.SourceLanguageCode;
                    TargetLanguage = glossary.LanguagePair.TargetLanguageCode;
                    break;
                default:
                    Type = "?";
                    SourceLanguage = string.Empty;
                    TargetLanguage = string.Empty;
                    break;
            }
            InputUri = glossary.InputConfig.GcsSource.InputUri;
            _Entries = glossary.EntryCount;
            OnPropertyChanged();
        }

        internal string OperationName;

        private GlossaryStatus _Status;

        public GlossaryStatus Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged();
            }
        }

        public string StatusString
        {
            get
            {
                switch (Status)
                {
                    case GlossaryStatus.None: return string.Empty;
                    case GlossaryStatus.Creating: return "Creating";
                    case GlossaryStatus.Deleting: return "Deleting";
                    case GlossaryStatus.Faint: return "(removing)";
                    case GlossaryStatus.Error: return "Error";
                    default:
                        throw new ApplicationException($"Invalid Status ({Status})");
                }
            }
        }

        public string GlossaryID { get; private set; }

        public string Type { get; private set; }

        public string SourceLanguage { get; private set; }

        public string TargetLanguage { get; private set; }

        public string InputUri { get; private set; }

        private int _Entries;

        public int? Entries => _Status == GlossaryStatus.None ? _Entries : (int?)null;

        public bool Alert => Status == GlossaryStatus.Error;

        public string AlertMessage { get; set; }

        public bool IsDeletable => _Status == GlossaryStatus.None || _Status == GlossaryStatus.Creating;

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs PropertyChangedEventArgs
            = new PropertyChangedEventArgs(null);

        protected virtual void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, PropertyChangedEventArgs);
        }

        #endregion
    }
}
