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

namespace DuDuChinese.ViewModels
{
    public class ExercisePageViewModel : ViewModelBase
    {
        private DictionaryItem currentItem = null;
        public DictionaryItem CurrentItem
        {
            get { return this.currentItem; }
            set { this.Set(ref this.currentItem, value); }
        }

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

        private static readonly Brush defaultTextColor =
            (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
            new SolidColorBrush(Windows.UI.Colors.White) : new SolidColorBrush(Windows.UI.Colors.Black);
        private static readonly Brush transparentBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
        private static readonly Brush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static readonly Brush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        private Brush bgcolour = transparentBrush;
        public Brush BgColour
        {
            get { return this.bgcolour; }
            set { this.Set(ref this.bgcolour, value); }
        }

        private Brush fgcolour = defaultTextColor;
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

        // Visibility flags
        private Visibility pinyinVisible = Visibility.Collapsed;
        public Visibility PinyinVisible
        {
            get { return this.pinyinVisible; }
            set { this.Set(ref this.pinyinVisible, value); }
        }

        private Visibility translationVisible = Visibility.Collapsed;
        public Visibility TranslationVisible
        {
            get { return this.translationVisible; }
            set { this.Set(ref this.translationVisible, value); }
        }

        private Visibility simplifiedVisible = Visibility.Collapsed;
        public Visibility SimplifiedVisible
        {
            get { return this.simplifiedVisible; }
            set { this.Set(ref this.simplifiedVisible, value); }
        }

        public ExercisePageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();

            // This is our exercise! :)
            CurrentItem = LearningEngine.GetNextItem();
            Summary = LearningEngine.GetStatus();

            // Later put here exercise selection
            // ....

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
            DictionaryItem nextItem = LearningEngine.GetNextItem();

            if (nextItem == null)
            {
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
            else
            {
                CurrentItem = nextItem;
                Summary = LearningEngine.GetStatus();
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

            this.PinyinVisible = Visibility.Visible;
            this.SimplifiedVisible = Visibility.Visible;
            this.TranslationVisible = Visibility.Visible;
            this.Validated = true;
            this.InputTextDisabled = true;
        }

        private void ResetUI()
        {
            this.Status = LearningEngine.GetDescription(LearningEngine.CurrentExercise) + ":";
            this.BgColour = transparentBrush;
            this.FgColour = defaultTextColor;
            this.InputTextDisabled = (LearningEngine.CurrentExercise == LearningExercise.Display);
            this.Validated = false;
            this.InputText = "";

            // Set visibility
            LearningEngine.SetVisibility(ref this.pinyinVisible, ref this.translationVisible, ref this.simplifiedVisible);
            this.PinyinVisible = this.pinyinVisible;
            this.TranslationVisible = this.translationVisible;
            this.SimplifiedVisible = this.simplifiedVisible;
        }
    }
}

