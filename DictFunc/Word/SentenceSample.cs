using System;
using System.Collections.Generic;
using System.Text;

namespace DictFunc.Word
{
    public class SentenceSample
    {
        public string English { get; }
        public string Chinese { get; }
        public Uri AudioUri { get; }
        public Uri VideoUri { get; }

        public bool HasAudio => AudioUri != null;
        public bool HasVideo => VideoUri != null;

        internal SentenceSample(string english, string chinese, Uri audioUri, Uri videoUri)
        {
            if (string.IsNullOrWhiteSpace(english))
                throw new ArgumentNullException(nameof(english));
            if (string.IsNullOrWhiteSpace(chinese))
                throw new ArgumentNullException(nameof(chinese));
            English = english;
            Chinese = chinese;
            AudioUri = audioUri;
            VideoUri = videoUri;
        }
    }
}
