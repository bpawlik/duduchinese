using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using DuDuChinese.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DuDuChinese.ViewModels
{
    public class NewMaterialPageViewModel : ViewModelBase
    {
        public NewMaterialPageViewModel()
        {
        }

        public ObservableCollection<DictionaryItemList> Lists { get; private set; } = null;

        private int selectedListIndex = -1;
        public int SelectedListIndex
        {
            get { return this.selectedListIndex; }
            set { this.Set(ref this.selectedListIndex, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
            }

            DictionaryManager.Deserialize();

            // Reset learning engine
            LearningEngine.Reset();

            if (this.Lists == null)
                this.Lists = new ObservableCollection<DictionaryItemList>();

            // Refresh list (always)
            this.Lists.Clear();
            foreach (var key in DictionaryManager.Lists.Keys)
                this.Lists.Add(DictionaryManager.Lists[key]);

            // Select index
            if (this.Lists.Count > 0)
                this.SelectedListIndex = 0;

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

        public void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            LearningEngine.Mode = rb.Tag == null ? LearningMode.Words : (LearningMode)Convert.ToInt32(rb.Tag);
        }

        public async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (LearningEngine.CurrentItemList == null)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Content = "Learning list is empty!";
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "OK";
                dialog.CanDrag = true;
                
                await dialog.ShowAsync();
            }
            else
            {
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
        }

        public void SelectedListChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            LearningEngine.CurrentItemList = (DictionaryItemList)cb.SelectedItem;
            this.selectedListIndex = cb.SelectedIndex;
        }

        //public void GotoDetailsPage() =>
        //    NavigationService.Navigate(typeof(Views.DetailPage), Value);

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

    }
}

