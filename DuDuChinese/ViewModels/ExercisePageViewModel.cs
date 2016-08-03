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

namespace DuDuChinese.ViewModels
{
    public class ExercisePageViewModel : ViewModelBase
    {
        private DictionaryItem currentItem = null;
        public DictionaryItem CurrentItem
        {
            get { return this.currentItem; }
            set
            {
                this.Set(ref this.currentItem, value);
            }
        }

        private string status = "Enter translation:";
        public string Status
        {
            get { return this.status; }
            set
            {
                this.Set(ref this.status, value);
            }
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

            // Set visibility
            LearningEngine.SetVisibility(ref this.pinyinVisible, ref this.translationVisible, ref this.simplifiedVisible);

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
                // Validate the answer
                //LearningEngine.Validate()

                this.Status = "Very good!";
            }
        }
    }
}

