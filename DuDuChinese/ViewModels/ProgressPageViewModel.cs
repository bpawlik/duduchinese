using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;
using Windows.UI.Xaml;
using System.ComponentModel;

namespace DuDuChinese.ViewModels
{
    public class ProgressItem : ViewModelBase
    {
        private string text = "";
        public string Text
        {
            get { return this.text; }
            set
            {
                this.Set(ref this.text, value);
            }
        }

        private static readonly Windows.UI.Color defaultColor =
            (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
            Windows.UI.Colors.White : Windows.UI.Colors.Black;


        private Windows.UI.Color foregroundColor = defaultColor;
        public Windows.UI.Color ForegroundColor
        {
            get { return this.foregroundColor; }
            set
            {
                this.Set(ref this.foregroundColor, value);
            }
        }

        public ProgressItem(string text)
        {
            this.text = text;
        }
    }

    public class ProgressPageViewModel : ViewModelBase
    {
        public int SelectedItemIndex { get; set; } = -1;
        public List<ProgressItem> ProgressItems { get; set; } = null;
        public string Text { get; set; } = "Click Continue to go to the next exercise:";
        public bool CancelEnabled { get; set; } = true;

        public ProgressPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();

            // Refresh task list
            if (this.ProgressItems == null)
                this.ProgressItems = new List<ProgressItem>();

            foreach (LearningExercise exerc in LearningEngine.ExerciseList)
            {
                this.ProgressItems.Add(new ProgressItem(LearningEngine.GetDescription(exerc)));
            }

            LearningExercise nextExcercise = LearningEngine.NextExercise();
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
                this.SelectedItemIndex = LearningEngine.CurrentExerciseIndex;
            }

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
            if (LearningEngine.CurrentExercise == LearningExercise.Done)
            {
                // We're done!
                // Go back to new material page
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
            NavigationService.Navigate(typeof(Views.NewMaterialPage), 0);
        }

        public void listView_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            // Color green all passed items

            for (int i = 0; i < LearningEngine.CurrentExerciseIndex; i++)
                    ProgressItems[i].ForegroundColor = Windows.UI.Colors.Green;
        }
    }
}

