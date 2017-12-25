using Hake.Extension.ValueRecord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class WordDefinitionsCollection : IReadOnlyList<WordDefinition>
    {
        public WordDefinition this[int index] => words[index];
        public int Count => words.Count;


        private List<WordDefinition> words;
        private WordDefinitionsCollection(List<WordDefinition> words)
        {
            this.words = words;
        }

        public IEnumerator<WordDefinition> GetEnumerator() => words.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => words.GetEnumerator();

        internal static WordDefinitionsCollection FromRecord(ListRecord list)
        {
            List<WordDefinition> words = new List<WordDefinition>();
            if (list == null)
                return new WordDefinitionsCollection(words);

            foreach (RecordBase record in list)
            {
                if (record is SetRecord set)
                {
                    if (set.TryGetValue("pos", out RecordBase posRec) &&
                        set.TryGetValue("def", out RecordBase defRec) &&
                        posRec is ScalerRecord pos &&
                        defRec is ScalerRecord def)
                    {
                        string posValue = pos.ReadAs<string>();
                        string defValue = def.ReadAs<string>();
                        words.Add(new WordDefinition(posValue, defValue));
                    }
                }
            }
            return new WordDefinitionsCollection(words);
        }
    }
}
