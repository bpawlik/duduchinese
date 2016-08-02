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

namespace DuDuChinese.ViewModels
{
    public class ProgressPageViewModel : ViewModelBase
    {
        public List<string> ExerciseList { get; set; } = null;
        public int SelectedItemIndex { get; set; } = -1;

        public ProgressPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();

            // Refresh task list
            if (this.ExerciseList == null)
                this.ExerciseList = new List<string>();

            this.ExerciseList.Clear();
            foreach (LearningExercise exerc in LearningEngine.ExerciseList)
                this.ExerciseList.Add(LearningEngine.GetDescription(exerc));

            int index = LearningEngine.NextExercise();
            if (index < 0)
            {
                // This is it, display summary and ask whether one want to review
            }
            else
            {
                this.SelectedItemIndex = index;
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
            if (LearningEngine.CurrentExerciseIndex == LearningEngine.ExerciseList.Count)
            {
                // We're done!
                // Go back to new material page
                NavigationService.Navigate(typeof(Views.NewMaterialPage), 0);
            }
            else
            {
                NavigationService.Navigate(typeof(Views.ExerciseDisplayPage), 0);
            }
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.ProgressPage), 0);
        }
    }
}

