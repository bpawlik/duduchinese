using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;
using Windows.UI.Xaml;

namespace DuDuChinese.ViewModels
{
    public class ProgressItem : ViewModelBase
    {
        public ProgressItem() { }

        private string text = "";
        public string Text
        {
            get { return this.text; }
            set { this.Set(ref this.text, value); }
        }

        private int itemCount = 0;
        public int ItemCount
        {
            get { return this.itemCount; }
            set { this.Set(ref this.itemCount, value); }
        }

        private bool isChecked = true;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set { this.Set(ref this.isChecked, value); }
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.Set(ref this.isEnabled, value); }
        }

        private Windows.UI.Color foregroundColor = Windows.UI.Colors.Black;
        public Windows.UI.Color ForegroundColor
        {
            get { return this.foregroundColor; }
            set { this.Set(ref this.foregroundColor, value); }
        }

        public LearningExercise Exercise { get; set; }
    }

    public class ProgressPageViewModel : ViewModelBase
    {
        public int SelectedItemIndex { get; set; } = -1;
        public List<ProgressItem> ProgressItems { get; set; } = null;
        public string Text { get; set; } = "Click Continue to go to the next exercise:";
        public bool CancelEnabled { get; set; } = true;
        private int nextExerciseIndex = -1;

        public string PageTitle
        {
            get { return LearningEngine.Mode == LearningMode.Revision ? "Revision" : "New Material"; }
        }

        public ProgressPageViewModel() {}

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Hide shell
            Views.Shell.HamburgerMenu.IsFullScreen = true;

            // Serialize revision engine
            RevisionEngine.Serialize();

            // Peek next exercise
            LearningExercise nextExcercise = LearningEngine.PeekNextExercise(out this.nextExerciseIndex);

            // Refresh task list
            if (this.ProgressItems == null)
                this.ProgressItems = new List<ProgressItem>();
            foreach (LearningExercise exerc in LearningEngine.ExerciseList)
            {
                this.ProgressItems.Add(new ProgressItem()
                {
                    Exercise = exerc,
                    Text = LearningEngine.GetDescription(exerc),
                    ItemCount = LearningEngine.GetItemCountForExercise(exerc),
                    IsEnabled = (LearningEngine.Mode != LearningMode.Revision)
                });
            }

            if (nextExcercise == LearningExercise.Done)
            {
                // This is it, display summary and ask whether one want to review
                foreach (var item in ProgressItems)
                    item.ForegroundColor = Windows.UI.Colors.Green;

                this.Text = "All done! Click Continue to go back to the New Material page:";
                this.CancelEnabled = false;
            }
            else
            {
                this.SelectedItemIndex = this.nextExerciseIndex;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void Continue_Click(object sender, RoutedEventArgs e)
        {
            // Update exercise list based on the checkbox selection
            if (LearningEngine.Mode != LearningMode.Revision)
                foreach (var item in ProgressItems)
                    if (!item.IsChecked)
                        LearningEngine.ExerciseList.Remove(item.Exercise);

            // Move to the next exercise
            LearningEngine.NextExercise();

            if (LearningEngine.CurrentExercise == LearningExercise.Done)
            {
                // We're done!
                // Go back to new material / revision page
                if (LearningEngine.Mode == LearningMode.Revision)
                    NavigationService.Navigate(typeof(Views.RevisePage), 0);
                else
                    NavigationService.Navigate(typeof(Views.NewMaterialPage), 0);
            }
            else
            {
                switch (LearningEngine.CurrentExercise)
                {
                    case LearningExercise.Display:
                        NavigationService.Navigate(typeof(Views.ExerciseDisplayPage), 0);
                        break;
                    case LearningExercise.HanziPinyin2English:
                    case LearningExercise.Hanzi2Pinyin:
                    case LearningExercise.Pinyin2English:
                    case LearningExercise.Pinyin2Hanzi:
                    case LearningExercise.English2Hanzi:
                    case LearningExercise.English2Pinyin:
                        NavigationService.Navigate(typeof(Views.ExerciseTextBoxPage), 0);
                        break;
                }
            }
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (LearningEngine.Mode == LearningMode.Revision)
                NavigationService.Navigate(typeof(Views.RevisePage), 0);
            else
                NavigationService.Navigate(typeof(Views.NewMaterialPage), 0);
        }

        public void listView_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            // Color all items to the default color
            Windows.UI.Color defaultColor = (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
                Windows.UI.Colors.White : Windows.UI.Colors.Black;
            foreach (var item in ProgressItems)
                item.ForegroundColor = defaultColor;

            // Color green all passed items
            for (int i = 0; i < this.nextExerciseIndex; i++)
            {
                ProgressItems[i].ForegroundColor = Windows.UI.Colors.Green;
                ProgressItems[i].IsEnabled = false;
            }
                
        }
    }
}

