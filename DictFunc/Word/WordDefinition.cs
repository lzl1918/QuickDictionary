using System;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class WordDefinition
    {
        public string Position { get; }
        public string Definition { get; }

        internal WordDefinition(string position, string definition)
        {
            if (string.IsNullOrWhiteSpace(position))
                throw new ArgumentNullException(nameof(position));
            if (string.IsNullOrWhiteSpace(definition))
                throw new ArgumentNullException(nameof(definition));
            Position = position;
            Definition = definition;
        }
    }
}
