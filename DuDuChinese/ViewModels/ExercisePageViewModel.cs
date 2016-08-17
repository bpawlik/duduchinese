using DuDuChinese.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using CC_CEDICT.Universal;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;

namespace DuDuChinese.ViewModels
{
    public class ExercisePageViewModel : ViewModelBase
    {
        #region Brushes

        private static readonly Brush whiteBrush = new SolidColorBrush(Windows.UI.Colors.White);
        private static readonly Brush blackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static readonly Brush transparentBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
        private static readonly Brush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static readonly Brush greenBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x32, 0xCD, 0x32));

        private Brush bgcolour = transparentBrush;
        public Brush BgColour
        {
            get { return this.bgcolour; }
            set { this.Set(ref this.bgcolour, value); }
        }

        private Brush fgcolour = blackBrush;
        public Brush FgColour
        {
            get { return this.fgcolour; }
            set { this.Set(ref this.fgcolour, value); }
        }

        #endregion

        #region UI Controls

        private static TextBlock GetErrorTextBlock(string message)
        {
            return new TextBlock()
            {
                Text = message,
                Foreground = redBrush,
                FontSize = 24.0,
                Margin = new Thickness(10)
            };
        }

        private static TextBlock GetTextBlock(string text)
        {
            return new TextBlock()
            {
                Text = text,
                FontSize = 24.0,
                Margin = new Thickness(-10, 0, -10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        public static TextBox GetTextBox()
        {
            return new TextBox()
            {
                Width = 125.0,
                Margin = new Thickness(-5, 0, -5, 0),
                FontSize = 24.0,
                IsSpellCheckEnabled = false,
                AcceptsReturn = false,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        #endregion

        public ObservableCollection<ItemViewModel> Items { get; private set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<object> SentenceItems { get; private set; } = new ObservableCollection<object>();
        public TextBox KeywordTextBox { get; set; } = null;
        public bool Validated { get; set; } = false;
        public bool InputTextDisabled { get; set; } = false;
        public MediaElement Media { get; internal set; }

        #region RaisePropertyChanged properties

        private DictionaryRecord currentItem = null;
        public DictionaryRecord CurrentItem
        {
            get { return this.currentItem; }
            set {
                this.Items.Clear();

                if (value == null)
                    return;

                Visibility pinyinVisible, translationVisible, simplifiedVisible;
                LearningEngine.SetVisibility(out pinyinVisible, out translationVisible, out simplifiedVisible);
                this.Items.Add(new ItemViewModel()
                {
                    Record = value,
                    Pinyin = value.Chinese.Pinyin,
                    English = String.Join("; ", value.English),
                    EnglishWithNewlines = String.Join("\n", value.English),
                    Chinese = value.Chinese.Simplified,
                    Index = value.Index,
                    PinyinVisible = pinyinVisible,
                    TranslationVisible = translationVisible,
                    SimplifiedVisible = simplifiedVisible,
                    SentenceVisible = simplifiedVisible,
                    Sentence = String.Join(Environment.NewLine + " - ", value.Sentence)
                });
                this.Set(ref this.currentItem, value);

                if (LearningEngine.CurrentExercise == LearningExercise.Display)
                    Play(LearningEngine.Mode == LearningMode.Sentences);   
            }
        }

        private string status = "Enter translation:";
        public string Status
        {
            get { return this.status; }
            set { this.Set(ref this.status, value); }
        }

        public string inputText = "";
        public string InputText
        {
            get { return this.inputText; }
            set { this.Set(ref this.inputText, value); }
        }

        private string summary = "Exercises started.";
        public string Summary
        {
            get { return this.summary; }
            set { this.Set(ref this.summary, value); }
        }

        private int progressValue = 1;
        public int ProgressValue
        {
            get { return this.progressValue; }
            set { this.Set(ref this.progressValue, value); }
        }

        private int progressMaxValue = 1;
        public int ProgressMaxValue
        {
            get { return this.progressMaxValue; }
            set { this.Set(ref this.progressMaxValue, value); }
        }

        private bool isWrongAnswer = false;
        public bool IsWrongAnswer
        {
            get { return this.isWrongAnswer; }
            set { this.Set(ref this.isWrongAnswer, value); }
        }

        #endregion

        public ExercisePageViewModel() {}

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Hide shell
            Views.Shell.HamburgerMenu.IsFullScreen = true;

            // This is our exercise! :)
            CurrentItem = LearningEngine.GetNextItem();
            Summary = LearningEngine.GetStatus();

            this.ProgressValue = 1;
            this.ProgressMaxValue = LearningEngine.GetItemCountForCurrentExercise();

            ResetUI();

            if (LearningEngine.Mode == LearningMode.Sentences)
                ShowFillGapTextBox();

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Dissect sentence into List<TextBlock> before,  TextBox keyword, List<TextBlock> after
        /// </summary>
        private void ShowFillGapTextBox()
        {
            if (this.currentItem.Sentence.Count == 0)
                return;

            string sentence, keyword = "";
            switch (LearningEngine.CurrentExercise)
            {
                case LearningExercise.FillGapsChinese:
                    sentence = this.currentItem.Sentence[0];
                    keyword = this.currentItem.Chinese.Simplified;
                    break;
                case LearningExercise.FillGapsEnglish:
                    sentence = this.currentItem.Sentence[1];
                    // TODO: add logic in case of abbreviations, apostrophes, change of person, etc
                    foreach (string word in this.currentItem.English)
                        if (sentence.Contains(word))
                        {
                            keyword = word;
                            break;
                        }
                    break;
                default:
                    return;
            }

            if (String.IsNullOrWhiteSpace(sentence) || String.IsNullOrWhiteSpace(keyword))
            {
                SentenceItems.Add(GetErrorTextBlock("Keyword and/or sentence is empty!"));
                return;
            }

            int idx = sentence.IndexOf(keyword);
            if (idx == -1)
            {
                SentenceItems.Add(GetErrorTextBlock("Keyword not found in the sentence!"));
                return;
            }

            string[] before = sentence.Substring(0, idx).Split(new char[] {' ', ',' });
            string[] after = sentence.Substring(idx + keyword.Length, sentence.Length - idx - keyword.Length).Split(' ');

            // Add words before the keyword
            foreach (string item in before)
                SentenceItems.Add(GetTextBlock(item));

            // Add keyword
            SentenceItems.Add(this.KeywordTextBox);

            // Add words after the keyword
            foreach (string item in after)
                SentenceItems.Add(GetTextBlock(item));

            this.KeywordTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        public void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (!Validated && !InputTextDisabled)
            {
                if (LearningEngine.CurrentExercise != LearningExercise.Display)
                    Play(LearningEngine.Mode == LearningMode.Sentences);

                if (e.OriginalSource.GetType() == typeof(Windows.UI.Xaml.Controls.TextBox))
                    Validate((e.OriginalSource as Windows.UI.Xaml.Controls.TextBox).Text);
                else
                    Validate();
                return;
            }

            ResetUI();
            DictionaryRecord nextItem = LearningEngine.GetNextItem();

            if (nextItem == null)
            {
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
            else
            {
                CurrentItem = nextItem;
                this.ProgressMaxValue = LearningEngine.GetItemCountForCurrentExercise();
                Summary = LearningEngine.GetStatus();
                this.ProgressValue++;

                if (LearningEngine.Mode == LearningMode.Sentences)
                    ShowFillGapTextBox();
            }
        }

        public void Accept_Click(object sender, RoutedEventArgs e)
        {
            // Need to revert Validate result
            LearningEngine.RevertLastValidate();

            this.Validated = true;
            Continue_Click(sender, e);
        }

        // TextBox key down
        public void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Validate((sender as Windows.UI.Xaml.Controls.TextBox).Text);
            }
        }

        private void Validate(string input = null)
        {
            // Validate the answer
            bool pass = LearningEngine.Validate(input == null ? this.InputText : input);
            this.IsWrongAnswer = !pass;

            if (pass)
            {
                this.Status = "Very good!";
                this.FgColour = greenBrush;
                this.BgColour = greenBrush;
            }
            else
            {
                this.Status = "Correct answer:";
                this.BgColour = redBrush;
                this.FgColour = redBrush;
            }

            if (LearningEngine.Mode == LearningMode.Sentences)
            {
                // Find textbox and colour it!
                foreach (var control in this.SentenceItems)
                    if (control is TextBox)
                        (control as TextBox).Background = this.BgColour;
            }

            // Set visibility
            if (this.Items.Count > 0)
            {
                this.Items[0].PinyinVisible = Visibility.Visible;
                this.Items[0].TranslationVisible = Visibility.Visible;
                this.Items[0].SimplifiedVisible = Visibility.Visible;
                this.Items[0].SentenceVisible = Visibility.Visible;
            }
            this.Validated = true;
            this.InputTextDisabled = true;
        }

        private void ResetUI()
        {
            Brush defaultTextColor = (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
                whiteBrush : blackBrush;

            this.Status = LearningEngine.GetDescription(LearningEngine.CurrentExercise) + ":";
            this.BgColour = transparentBrush;
            this.FgColour = defaultTextColor;
            this.InputTextDisabled = (LearningEngine.CurrentExercise == LearningExercise.Display);
            this.Validated = false;
            this.InputText = "";
            this.IsWrongAnswer = false;
            this.SentenceItems.Clear();
            if (this.KeywordTextBox != null)
            {
                this.KeywordTextBox.Text = "";
                this.KeywordTextBox.Background = transparentBrush;
                this.KeywordTextBox.Foreground = defaultTextColor;
            }

            // Set visibility
            if (this.Items.Count > 0)
            {
                Visibility pinyinVisible, translationVisible, simplifiedVisible;
                LearningEngine.SetVisibility(out pinyinVisible, out translationVisible, out simplifiedVisible);
                ItemViewModel c = this.Items[0];
                this.Items[0].PinyinVisible = pinyinVisible;
                this.Items[0].TranslationVisible = translationVisible;
                this.Items[0].SimplifiedVisible = simplifiedVisible;
                this.Items[0].SentenceVisible = simplifiedVisible;
            }
        }

        public async void Play(bool sentence = false)
        {
            // If the media is playing, the user has pressed the button to stop the playback.
            if (Media.CurrentState.Equals(MediaElementState.Playing))
            {
                Media.Stop();

                // Wait for a while
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                // Try again
                if (Media.CurrentState.Equals(MediaElementState.Playing))
                    return;
            }

            string text = (sentence && this.currentItem.Sentence.Count > 0) ? this.currentItem.Sentence[0] : this.currentItem.Chinese.Simplified;

            if (!String.IsNullOrEmpty(text))
            {
                try
                {
                    // Create a stream from the text. This will be played using a media element.
                    App app = (App)Application.Current;
                    SpeechSynthesisStream synthesisStream = await app.Synthesizer.SynthesizeTextToStreamAsync(text);

                    // Set the source and start playing the synthesized audio stream.
                    Media.AutoPlay = true;
                    Media.SetSource(synthesisStream, synthesisStream.ContentType);
                    Media.Play();
                }
                catch (System.IO.FileNotFoundException)
                {
                    // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                    // be able to start media playback. Handle this gracefully
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                    await messageDialog.ShowAsync();
                }
                catch (Exception)
                {
                    // If the text is unable to be synthesized, throw an error message to the user.
                    Media.AutoPlay = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "Unable to synthesize text. Please download Chinese simplified speech language pack.");
                    await messageDialog.ShowAsync();
                }
            }
        }
    }
}

