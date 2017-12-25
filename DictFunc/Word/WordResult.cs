using Hake.Extension.ValueRecord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class WordResult
    {
        public string Word { get; }
        public WordPronunciationsCollection Pronunciations { get; }
        public WordDefinitionsCollection Definitions { get; }
        public SentencesCollection Samples { get; }
        private WordResult(string word, WordPronunciationsCollection pronunciations, WordDefinitionsCollection definitions, SentencesCollection samples)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            Word = word;
            Pronunciations = pronunciations;
            Definitions = definitions;
            Samples = samples;
        }

        internal static WordResult FromRecord(SetRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (record.TryGetValue("word", out RecordBase wordRecord) && wordRecord is ScalerRecord wordScaler)
            {
                if (wordScaler.ScalerType == ScalerType.Null)
                    return null;

                string word = wordScaler.ReadAs<string>().Trim();
                WordPronunciationsCollection pronunciationCollection = null;
                if (record.TryGetValue("pronunciation", out RecordBase pronunciation))
                    pronunciationCollection = WordPronunciationsCollection.FromRecord(pronunciation as SetRecord);
                else
                    pronunciationCollection = WordPronunciationsCollection.FromRecord(null);
                WordDefinitionsCollection definitionCollection = null;
                if (record.TryGetValue("defs", out RecordBase defs))
                    definitionCollection = WordDefinitionsCollection.FromRecord(defs as ListRecord);
                else
                    definitionCollection = WordDefinitionsCollection.FromRecord(null);
                SentencesCollection sentencesCollection = null;
                if (record.TryGetValue("sams", out RecordBase sentences))
                    sentencesCollection = SentencesCollection.FromRecord(sentences as ListRecord);
                else
                    sentencesCollection = SentencesCollection.FromRecord(null);
                return new WordResult(word, pronunciationCollection, definitionCollection, sentencesCollection);
            }
            else
                throw new Exception($"can not read {nameof(WordResult)} from record");
        }
        internal static WordResult FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));
            RecordBase record = Hake.Extension.ValueRecord.Json.Converter.ReadJson(json);
            if (record is SetRecord set)
                return FromRecord(set);
            else
                throw new Exception("invalid json string");
        }
    }
}
