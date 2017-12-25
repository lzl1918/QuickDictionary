using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DictFunc.Word
{
    public class WordRetrivalResult
    {
        public string RequestedWord { get; }
        public bool IsCancelled { get; }
        public bool IsExceptionThrown => Exception != null;
        public bool IsSuccess => !IsCancelled && !IsExceptionThrown;
        public Exception Exception { get; }
        public WordResult Result { get; }

        internal WordRetrivalResult(string word, WordResult result)
        {
            Result = result;
            RequestedWord = word;
        }
        internal WordRetrivalResult(string word, Exception exception)
        {
            Exception = exception;
            RequestedWord = word;
        }
        internal WordRetrivalResult(string word, bool cancelFlag)
        {
            IsCancelled = true;
            RequestedWord = word;
        }
    }

    public static class WordRetrivationService
    {
        private const string MAIN_TARGET = "http://xtk.azurewebsites.net/BingDictService.aspx";

        public static Task<WordRetrivalResult> RetriveWordAsync(string word, bool getSampleSentences, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            Task<WordRetrivalResult> task = new Task<WordRetrivalResult>(() =>
            {
                word = word.Trim();
                string encodedWord = Uri.EscapeDataString(word);
                string url = $"{MAIN_TARGET}?Word={encodedWord}";
                if (getSampleSentences)
                    url += "&Samples=true";
                else
                    url += "&Samples=false";

                HttpClient client = new HttpClient();
                string response;
                try
                {
                    HttpResponseMessage responseMessage = client.GetAsync(url, cancellationToken).GetAwaiter().GetResult();
                    if (!responseMessage.IsSuccessStatusCode)
                        throw new Exception("request failed");
                    response = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    WordResult result = WordResult.FromJson(response);
                    return new WordRetrivalResult(word, result);
                }
                catch (TaskCanceledException)
                {
                    return new WordRetrivalResult(word, true);
                }
                catch (Exception ex)
                {
                    return new WordRetrivalResult(word, ex);
                }
                finally
                {
                    client.Dispose();
                }
            }, cancellationToken);
            task.Start();
            return task;
        }
        public static Task<WordRetrivalResult> RetriveWordAsync(string word, bool getSampleSentences)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            Task<WordRetrivalResult> task = new Task<WordRetrivalResult>(() =>
            {
                word = word.Trim();
                string encodedWord = Uri.EscapeDataString(word);
                string url = $"{MAIN_TARGET}?Word={encodedWord}";
                if (getSampleSentences)
                    url += "&Samples=true";
                else
                    url += "&Samples=false";

                HttpClient client = new HttpClient();
                string response;
                try
                {
                    HttpResponseMessage responseMessage = client.GetAsync(url).GetAwaiter().GetResult();
                    if (!responseMessage.IsSuccessStatusCode)
                        throw new Exception("request failed");
                    response = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    WordResult result = WordResult.FromJson(response);
                    return new WordRetrivalResult(word, result);
                }
                catch (TaskCanceledException)
                {
                    return new WordRetrivalResult(word, true);
                }
                catch (Exception ex)
                {
                    return new WordRetrivalResult(word, ex);
                }
                finally
                {
                    client.Dispose();
                }
            });
            task.Start();
            return task;
        }
    }
}
