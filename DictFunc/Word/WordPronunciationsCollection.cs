using Hake.Extension.ValueRecord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class WordPronunciationsCollection
    {
        public WordPronunciation USPronunciation { get; }
        public WordPronunciation UKPronunciation { get; }
        public bool HasUSPronunciation => USPronunciation != null;
        public bool HasUKPronunciation => UKPronunciation != null;

        private WordPronunciationsCollection(WordPronunciation usPronunciation, WordPronunciation ukPronunciation)
        {
            USPronunciation = usPronunciation;
            UKPronunciation = ukPronunciation;
        }

        internal static WordPronunciationsCollection FromRecord(SetRecord set)
        {
            if (set == null)
                return new WordPronunciationsCollection(null, null);

            WordPronunciation us = null;
            if (set.TryGetValue("AmE", out RecordBase ameRecord) && ameRecord is ScalerRecord ameScaler &&
                set.TryGetValue("AmEmp3", out RecordBase ameMp3Record) && ameMp3Record is ScalerRecord ameMp3Scaler)
            {
                string ame = ameScaler.ReadAs<string>();
                if (ameMp3Scaler.ScalerType == ScalerType.Null)
                    us = new WordPronunciation(ame, null);
                else
                {
                    string uri = ameMp3Scaler.ReadAs<string>();
                    us = new WordPronunciation(ame, new Uri(uri, UriKind.Absolute));
                }
            }
            WordPronunciation uk = null;
            if (set.TryGetValue("BrE", out RecordBase breRecord) && breRecord is ScalerRecord breScaler &&
                set.TryGetValue("BrEmp3", out RecordBase breMp3Record) && breMp3Record is ScalerRecord breMp3Scaler)
            {
                string bre = breScaler.ReadAs<string>();
                if (breMp3Scaler.ScalerType == ScalerType.Null)
                    uk = new WordPronunciation(bre, null);
                else
                {
                    string uri = breMp3Scaler.ReadAs<string>();
                    uk = new WordPronunciation(bre, new Uri(uri, UriKind.Absolute));
                }
            }
            return new WordPronunciationsCollection(us, uk);
        }
    }
}
