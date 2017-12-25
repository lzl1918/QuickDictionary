using System;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class WordPronunciation
    {
        public string PhoneticSymbol { get; }
        public Uri AudioUri { get; }

        public bool HasPhonetic => PhoneticSymbol != null;
        public bool HasAudio => AudioUri != null;

        internal WordPronunciation(string phoneticSymbol, Uri audioUri)
        {
            if (string.IsNullOrWhiteSpace(phoneticSymbol))
                PhoneticSymbol = null;
            else
                PhoneticSymbol = phoneticSymbol;
            AudioUri = audioUri;
        }
    }
}
