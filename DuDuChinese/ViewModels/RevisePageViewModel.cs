using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;
using Windows.UI.Xaml.Controls;

namespace DuDuChinese.ViewModels
{
    public class RevisePageViewModel : ViewModelBase
    {
        public RevisePageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
        }

        string _Value = "Blah";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }

            RevisionEngine.Deserialize();

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

        private int NumberOfItems { get; set; } = 0;

        public void NumberOfItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this.NumberOfItems = Convert.ToInt32((cb.SelectedItem as ComboBoxItem).Content.ToString());
        }

        public async void Start_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            List<RevisionItem> revisionList = RevisionEngine.GetRevisionList(this.NumberOfItems);

            if (revisionList == null || revisionList.Count == 0 || this.NumberOfItems == 0)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Content = "Revision list is empty!";
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "OK";
                dialog.CanDrag = true;

                await dialog.ShowAsync();
            }
            else
            {
                LearningEngine.UpdateRevisionList(revisionList);
                LearningEngine.Mode = LearningMode.Revision;
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
        }

    }
}

