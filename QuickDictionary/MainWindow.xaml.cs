using DictFunc.Word;
using QuickDictionary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickDictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush wordHighlightedBrush;
        private DataTemplate wordDefinitionTemplate;
        private DataTemplate sentenceDefinitionTemplate;
        private AutoResetEvent retrivationTaskWaiter;
        private AutoResetEvent setupRetrivationMutex;
        private CancellationTokenSource cancellationTokenSource;
        private IntPtr hwnd;

        public MainWindow()
        {
            retrivationTaskWaiter = new AutoResetEvent(true);
            setupRetrivationMutex = new AutoResetEvent(true);

            InitializeComponent();
            wordDefinitionTemplate = Resources["WordDefinitionBlockTemplate"] as DataTemplate;
            sentenceDefinitionTemplate = Resources["SampleSentenceBlockTemplate"] as DataTemplate;
            wordHighlightedBrush = Resources["SentenceHighlightForeground"] as SolidColorBrush;

            Loaded += OnLoaded;
            MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            double y = e.GetPosition(this).Y;
            if (y > 50)
                return;

            this.DragMove();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            hwnd = new WindowInteropHelper(this).Handle;
            GlassHelper.EnableAero(hwnd);

            RetriveWordInformation("test");
        }

        ~MainWindow()
        {
            retrivationTaskWaiter.Dispose();
            setupRetrivationMutex.Dispose();
        }

        private void RetriveWordInformation(string word)
        {
            Task.Run(() =>
            {
                setupRetrivationMutex.WaitOne();

                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();

                retrivationTaskWaiter.WaitOne();
                cancellationTokenSource = new CancellationTokenSource();
                WordRetrivationService.RetriveWordAsync(word, true, cancellationTokenSource.Token).ContinueWith(tsk =>
                {
                    WordRetrivalResult result = tsk.Result;
                    if (result.IsSuccess)
                    {
                        if (result.Result == null)
                            Dispatcher.Invoke(() => ShowNoDefinitionHint(result.RequestedWord));
                        else
                            Dispatcher.Invoke(() => ApplyWordResult(result.Result));
                    }
                    else if (result.IsExceptionThrown)
                    {
                        Dispatcher.Invoke(() => SwitchToRetrivationFailedHint(result.RequestedWord));
                    }
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                    retrivationTaskWaiter.Set();
                });

                setupRetrivationMutex.Set();
            });
        }
        private void ShowNoDefinitionHint(string word)
        {

        }
        private void SwitchToRetrivationFailedHint(string word)
        {
            if (panelRetrivationFailed.Visibility != Visibility.Visible)
            {
                panelRetrivationFailed.Visibility = Visibility.Visible;
                viewerWordInformation.Visibility = Visibility.Collapsed;
            }

            textRetrivationFailed.Text = $"retrivation of {word} failed";
        }

        private void ApplyWordResult(WordResult wordResult)
        {
            if (viewerWordInformation.Visibility != Visibility.Visible)
            {
                viewerWordInformation.Visibility = Visibility.Visible;
                panelRetrivationFailed.Visibility = Visibility.Collapsed;
            }

            string word = wordResult.Word;
            textWordTitle.Text = word;

            WordPronunciationsCollection pron = wordResult.Pronunciations;
            if (pron.HasUKPronunciation)
            {
                buttonUKPron.Visibility = Visibility.Visible;

                textUKPhoentic.Text = $"[{pron.UKPronunciation.PhoneticSymbol}]";
            }
            else
            {
                buttonUKPron.Visibility = Visibility.Collapsed;
            }

            if (pron.HasUSPronunciation)
            {
                buttonUSPron.Visibility = Visibility.Visible;
                textUSPhoentic.Text = $"[{pron.USPronunciation.PhoneticSymbol}]";
            }
            else
            {
                buttonUSPron.Visibility = Visibility.Collapsed;
            }

            panelDefinitions.Children.Clear();
            Grid definitionBlock;
            foreach (WordDefinition definition in wordResult.Definitions)
            {
                definitionBlock = wordDefinitionTemplate.LoadContent() as Grid;
                (definitionBlock.FindName("textWordPosition") as TextBlock).Text = definition.Position;
                (definitionBlock.FindName("textWordDefinition") as Run).Text = definition.Definition;
                panelDefinitions.Children.Add(definitionBlock);
            }

            StackPanel sentenceBlock;
            Paragraph eng, chn;
            panelSentences.Children.Clear();
            foreach (SentenceSample sentence in wordResult.Samples)
            {
                sentenceBlock = sentenceDefinitionTemplate.LoadContent() as StackPanel;
                eng = (Paragraph)sentenceBlock.FindName("paragraphEnglish");
                chn = (Paragraph)sentenceBlock.FindName("paragraphChinese");
                RenderSentence(word, sentence, eng, chn);
                panelSentences.Children.Add(sentenceBlock);
            }
        }
        private void RenderSentence(string word, SentenceSample sentence, Paragraph english, Paragraph chinese)
        {
            string eng = sentence.English;
            Regex wordReg = new Regex(word, RegexOptions.IgnoreCase);
            MatchCollection matches = wordReg.Matches(eng);
            int lastIndex = 0;
            string substr;
            Run run;
            foreach (Match match in matches)
            {
                substr = eng.Substring(lastIndex, match.Index - lastIndex);
                if (substr.Length > 0)
                {
                    run = new Run() { Text = substr };
                    english.Inlines.Add(run);
                }

                if (match.Length > 0)
                {
                    run = new Run() { Text = match.Value };
                    ApplyHighlightStyle(run);
                    english.Inlines.Add(run);
                }
                lastIndex = match.Index + match.Length;
            }
            substr = eng.Substring(lastIndex);
            if (substr.Length > 0)
            {
                run = new Run() { Text = substr };
                english.Inlines.Add(run);
            }

            string chn = sentence.Chinese;
            run = new Run() { Text = chn };
            chinese.Inlines.Add(run);
        }
        private void ApplyHighlightStyle(Run run)
        {
            run.Foreground = wordHighlightedBrush;
            run.FontWeight = FontWeights.Bold;
        }

        private void OnQueryButtonClick(object sender, RoutedEventArgs e)
        {
            string text = inputWord.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
                RetriveWordInformation(text);
        }

        private void OnWordInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.Key == Key.Enter)
            {
                string text = inputWord.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    RetriveWordInformation(text);
                e.Handled = true;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Handled)
                return;
            if (!e.Data.GetDataPresent(DataFormats.Text))
                return;

            string str = (string)e.Data.GetData(DataFormats.Text);
            str = str.Trim();
            if (!string.IsNullOrWhiteSpace(str) && ShouldQueryWord(str))
            {
                inputWord.Text = str;
                RetriveWordInformation(str);
                e.Handled = true;
            }
        }

        private void OnWordInputDrop(object sender, DragEventArgs e)
        {
            if (e.Handled)
                return;
            if (!e.Data.GetDataPresent(DataFormats.Text))
                return;

            string str = (string)e.Data.GetData(DataFormats.Text);
            str = str.Trim();
            if (!string.IsNullOrWhiteSpace(str) && ShouldQueryWord(str))
            {
                inputWord.Text = str;
                RetriveWordInformation(str);
                e.Handled = true;
            }
        }

        private bool ShouldQueryWord(string word)
        {
            if (word.Length > 30) return false;
            if (!word.All(IsSupportedCharacterForWord)) return false;
            if (!char.IsLetter(word[0])) return false;
            return true;

        }

        private bool IsSupportedCharacterForWord(char ch)
        {
            if (char.IsLetterOrDigit(ch)) return true;
            if (ch == '\'') return true;
            if (ch == '.') return true;
            return false;
        }
    }
}
