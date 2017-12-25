using Hake.Extension.ValueRecord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class SentencesCollection : IReadOnlyList<SentenceSample>
    {
        public SentenceSample this[int index] => sentences[index];
        public int Count => sentences.Count;

        private List<SentenceSample> sentences;
        private SentencesCollection(List<SentenceSample> sentences)
        {
            this.sentences = sentences;
        }


        public IEnumerator<SentenceSample> GetEnumerator() => sentences.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => sentences.GetEnumerator();

        internal static SentencesCollection FromRecord(ListRecord list)
        {
            List<SentenceSample> sentences = new List<SentenceSample>();
            if (list == null)
                return new SentencesCollection(sentences);

            foreach (RecordBase record in list)
            {
                if (record is SetRecord set)
                {
                    if (set.TryGetValue("eng", out RecordBase engRecord) && engRecord is ScalerRecord engScaler &&
                        set.TryGetValue("chn", out RecordBase chnRecord) && chnRecord is ScalerRecord chnScaler &&
                        set.TryGetValue("mp3Url", out RecordBase mp3Record) && mp3Record is ScalerRecord mp3Scaler &&
                        set.TryGetValue("mp4Url", out RecordBase mp4Record) && mp4Record is ScalerRecord mp4Scaler)
                    {
                        string eng = engScaler.ReadAs<string>();
                        string chn = chnScaler.ReadAs<string>();
                        Uri mp3 = null;
                        if (mp3Scaler.ScalerType == ScalerType.String)
                            mp3 = new Uri(mp3Scaler.ReadAs<string>(), UriKind.Absolute);
                        Uri mp4 = null;
                        if (mp4Scaler.ScalerType == ScalerType.String)
                            mp4 = new Uri(mp4Scaler.ReadAs<string>(), UriKind.Absolute);
                        sentences.Add(new SentenceSample(eng, chn, mp3, mp4));
                    }
                }
            }
            return new SentencesCollection(sentences);
        }
    }
}
