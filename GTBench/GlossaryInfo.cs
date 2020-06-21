using Google.Cloud.Translate.V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBench
{
    public class GlossaryInfo : INotifyPropertyChanged
    {
        public GlossaryInfo(Glossary glossary)
        {
            Id = glossary.GlossaryName.GlossaryId;
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
            Entries = glossary.EntryCount;
            InputUri = glossary.InputConfig.GcsSource.InputUri;
        }

        public string Id { get; }

        public string Type { get; }

        public string SourceLanguage { get; }

        public string TargetLanguage { get; }

        public string InputUri { get; }

        private int _Entries;

        public int Entries
        {
            get { return _Entries; }
            set
            {
                if (value != _Entries)
                {
                    _Entries = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entries)));
                }
            }
        }

        private string _Status = string.Empty;

        public string Status
        {
            get { return _Status; }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
