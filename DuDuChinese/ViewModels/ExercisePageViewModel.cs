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
                    SimplifiedVisible = simplifiedVisible
                });
                this.Set(ref this.currentItem, value);
                Play();
            }
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; } = new ObservableCollection<ItemViewModel>();

        private string status = "Enter translation:";
        public string Status
        {
            get { return this.status; }
            set { this.Set(ref this.status, value); }
        }

        public bool Validated { get; set; } = false;

        public string inputText = "";
        public string InputText
        {
            get { return this.inputText; }
            set { this.Set(ref this.inputText, value); }
        }

        public bool InputTextDisabled { get; set; } = false;

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

        private string summary = "Exercises started.";
        public string Summary
        {
            get { return this.summary; }
            set { this.Set(ref this.summary, value); }
        }

        public MediaElement Media { get; internal set; }

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

        public void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (!Validated && !InputTextDisabled)
            {
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
                Summary = LearningEngine.GetStatus();
                this.ProgressValue++;
            }
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

            // Set visibility
            if (this.Items.Count > 0)
            {
                this.Items[0].PinyinVisible = Visibility.Visible;
                this.Items[0].TranslationVisible = Visibility.Visible;
                this.Items[0].SimplifiedVisible = Visibility.Visible;
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

            // Set visibility
            if (this.Items.Count > 0)
            {
                Visibility pinyinVisible, translationVisible, simplifiedVisible;
                LearningEngine.SetVisibility(out pinyinVisible, out translationVisible, out simplifiedVisible);
                ItemViewModel c = this.Items[0];
                this.Items[0].PinyinVisible = pinyinVisible;
                this.Items[0].TranslationVisible = translationVisible;
                this.Items[0].SimplifiedVisible = simplifiedVisible;
            }
        }

        public async void Play()
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

            string text = this.currentItem.Chinese.Simplified;

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

