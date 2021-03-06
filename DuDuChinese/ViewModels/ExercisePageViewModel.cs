﻿using DuDuChinese.Models;
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
            if (LearningEngine.CurrentExercise == LearningExercise.FillGapsChinese)
                return new TextBlock()
                {
                    Text = text,
                    FontSize = 24.0,
                    Margin = new Thickness(-8, 0, -8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
            else
                return new TextBlock()
                {
                    Text = text,
                    FontSize = 24.0,
                    Margin = new Thickness(-10, 0, -10, 0),
                    FontFamily = new FontFamily("Consolas"),
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

                Visibility pinyinVisible, translationVisible, simplifiedVisible, sentenceVisible, sentenceChineseVisible;
                LearningEngine.SetVisibility(out pinyinVisible, out translationVisible, out simplifiedVisible, out sentenceVisible, out sentenceChineseVisible);
                this.Items.Add(new ItemViewModel()
                {
                    Record = value,
                    Pinyin = value.Chinese.Pinyin,
                    English = String.Join("; ", value.English),
                    EnglishWithNewlines = String.Join("\n", value.English),
                    Chinese = value.Chinese.Simplified,
                    Index = value.Index,
                    Sentence = String.Join(Environment.NewLine + " - ", value.Sentence),
                    SentenceChinese = value.Sentence.Count > 0 ? value.Sentence[0] : "",
                    SentenceEnglish = value.Sentence.Count > 1 ? value.Sentence[1] : "",
                    SentencePinyin = value.Sentence.Count > 2 ? value.Sentence[2] : "",
                    PinyinVisible = pinyinVisible,
                    TranslationVisible = translationVisible,
                    SimplifiedVisible = simplifiedVisible,
                    SentenceVisible = sentenceVisible,
                    SentenceChineseVisible = sentenceChineseVisible
                });
                this.Set(ref this.currentItem, value);

                if (LearningEngine.CurrentExercise == LearningExercise.Display)
                    Play(LearningEngine.IsSentence, ignoreException: true);   
            }
        }

        private string inputWarning = "";
        public string InputWarning
        {
            get { return this.inputWarning; }
            set { this.Set(ref this.inputWarning, value); }
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

        private bool validated = false;
        public bool Validated
        {
            get { return this.validated; }
            set { this.Set(ref this.validated, value); }
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

            if (LearningEngine.IsSentence)
                ShowFillGapTextBox();

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (!suspending)
            {
                LearningEngine.ResetExerciseState();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #region String comparison helpers

        private int LevenshteinDistance(string s, string t)
        {
            // degenerate cases
            if (s == t) return 0;
            if (s.Length == 0) return t.Length;
            if (t.Length == 0) return s.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    var cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[t.Length];
        }

        /// <summary>
        /// Gets the similarity between two strings.
        /// All relation scores are in the [0, 1] range, 
        /// which means that if the score gets a maximum value (equal to 1) 
        /// then the two string are absolutely similar
        /// </summary>
        /// <param name="string1">The string1.</param>
        /// <param name="string2">The string2.</param>
        /// <returns></returns>
        public float CalculateSimilarity(string s1, string s2)
        {
            if ((s1 == null) || (s2 == null))
                return 0.0f;

            float dis = LevenshteinDistance(s1, s2);
            float maxLen = s1.Length;
            if (maxLen < s2.Length)
                maxLen = s2.Length;
            if (maxLen == 0.0F)
                return 1.0F;
            else
                return 1.0F - dis / maxLen;
        }

        private string RemoveSpecialCharacters(string text)
        {
            return text.Replace(",", "").Replace(".", "").Replace("!", "").Replace("?", "").Replace("(", "").Replace(")", "");
        }

        #endregion

        /// <summary>
        /// Dissect sentence into List<TextBlock> before,  TextBox keyword, List<TextBlock> after
        /// </summary>
        private async void ShowFillGapTextBox()
        {
            if (this.currentItem == null || this.currentItem.Sentence.Count == 0)
            {
                SentenceItems.Add(GetErrorTextBlock("Oops... Something went wrong!"));
                return;
            }

            string sentence, keyword = "";
            switch (LearningEngine.CurrentExercise)
            {
                case LearningExercise.FillGapsChinese:
                    sentence = this.currentItem.Sentence[0];
                    keyword = this.currentItem.Chinese.Simplified;
                    break;
                case LearningExercise.FillGapsEnglish:
                    sentence = this.currentItem.Sentence[1];
                    foreach (string word in sentence.Split(' '))
                    {
                        if (!String.IsNullOrEmpty(keyword))
                            break;

                        string wordNoSpecial = RemoveSpecialCharacters(word);

                        foreach (string item in this.currentItem.English)
                        {
                            string itemNoSpecial = RemoveSpecialCharacters(item).Trim();

                            if (sentence.Contains(itemNoSpecial))
                            {
                                keyword = itemNoSpecial;
                                break;
                            }

                            float similarity = CalculateSimilarity(itemNoSpecial, wordNoSpecial);
                            if (similarity > 0.5)
                            {
                                keyword = wordNoSpecial;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    return;
            }

            if (String.IsNullOrWhiteSpace(sentence) || String.IsNullOrWhiteSpace(keyword))
            {
                SentenceItems.Add(GetErrorTextBlock("Sentence is empty or keyword not found!"));
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

            // A little bit of sleep to make sure that control is setup and ready
            await Task.Delay(100);
            this.KeywordTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        public void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (!Validated && !InputTextDisabled)
            {
                if (LearningEngine.CurrentExercise != LearningExercise.Display)
                    Play(LearningEngine.IsSentence, ignoreException: true);

                if (e.OriginalSource.GetType() == typeof(Windows.UI.Xaml.Controls.TextBox))
                    Validate((e.OriginalSource as Windows.UI.Xaml.Controls.TextBox).Text);
                else
                    Validate();
                Summary = LearningEngine.GetStatus();
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

                if (LearningEngine.IsSentence)
                    ShowFillGapTextBox();
            }
        }

        public void Accept_Click(object sender, RoutedEventArgs e)
        {
            // Need to revert Validate result
            LearningEngine.RevertLastValidate(true);

            this.Validated = true;
            Continue_Click(sender, e);
        }

        public void Learnt_Click(object sender, RoutedEventArgs e)
        {
            // Mark last item as learnt (no more reviews)
            LearningEngine.MarkLastAsLearnt();

            this.Validated = true;
            Continue_Click(sender, e);
        }

        public void TextBox_TextChanged(string text)
        {
            // Check if the input text matches input method
            this.InputWarning = LearningEngine.CheckInputLanguage(text);
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

                // Play 'wrong' sound effect
                App app = (App)Application.Current;
                app.AppSoundEffects.Play(Services.SoundEfxEnum.WRONG);
            }

            if (LearningEngine.IsSentence)
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
                this.Items[0].SentenceChineseVisible = Visibility.Visible;
            }
            this.Validated = true;
            this.InputTextDisabled = true;
        }

        private void ResetUI()
        {
            Brush defaultTextColor = (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
                whiteBrush : blackBrush;

            this.Status = LearningEngine.GetDescription<Command>(LearningEngine.CurrentExercise) + ":";
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
                Visibility pinyinVisible, translationVisible, simplifiedVisible, sentenceVisible, sentenceChineseVisible;
                LearningEngine.SetVisibility(out pinyinVisible, out translationVisible, out simplifiedVisible, out sentenceVisible, out sentenceChineseVisible);
                ItemViewModel c = this.Items[0];
                this.Items[0].PinyinVisible = pinyinVisible;
                this.Items[0].TranslationVisible = translationVisible;
                this.Items[0].SimplifiedVisible = simplifiedVisible;
                this.Items[0].SentenceVisible = sentenceVisible;
                this.Items[0].SentenceChineseVisible = sentenceChineseVisible;
            }
        }

        public async void ShowStrokeOrder()
        {
            if (this.currentItem == null)
                return;

            var dialog = new DuDuChinese.Views.Controls.StrokeOrderDialog(this.currentItem.Chinese);
            var result = await dialog.ShowAsync();
        }

        public async void Play(bool sentence = false, bool ignoreException = false)
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

            if (this.currentItem == null)
                return;

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
                    if (ignoreException)
                        return;
                    // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                    // be able to start media playback. Handle this gracefully
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                    await messageDialog.ShowAsync();
                }
                catch (Exception)
                {
                    // If the text is unable to be synthesized, throw an error message to the user.
                    Media.AutoPlay = false;
                    if (ignoreException)
                        return;
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "\nUnable to synthesize text. Please download Chinese simplified speech language pack.\n\n" +
                        "In order to do that go to Region & language -> Add a language -> Chinese (Simplified)\n\n" +
                        "Choose newly installed language from the list, select Options -> Speech -> Download.");
                    messageDialog.Title = "Language Pack missing";
                    await messageDialog.ShowAsync();
                }
            }
        }
    }
}

