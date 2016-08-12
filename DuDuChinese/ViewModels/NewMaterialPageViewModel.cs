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
        public ObservableCollection<string> Items { get; private set; }

        public bool IsStartEnabled { get; set; } = false;

        public NewMaterialPageViewModel()
        {
            this.Items = new ObservableCollection<string>();
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Restore shell
            Views.Shell.HamburgerMenu.IsFullScreen = false;

            this.IsStartEnabled = false;

            // Reset learning engine
            LearningEngine.Reset();
            LearningEngine.Mode = LearningMode.Words;

            App app = (App)Application.Current;
            this.Items.Clear();
            foreach (string key in app.ListManager.Keys)
            {
                this.Items.Add(key);
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

        public void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            LearningEngine.Mode = rb.Tag == null ? LearningMode.Words : (LearningMode)Convert.ToInt32(rb.Tag);
        }

        public async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (LearningEngine.LearningItems == null)
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

        public int SelectedListChanged(object sender)
        {
            this.IsStartEnabled = false;
            ComboBox cb = sender as ComboBox;
            App app = (App)Application.Current;
            if (cb.SelectedValue != null)
            {
                DictionaryRecordList recordList = app.ListManager[cb.SelectedValue.ToString()];
                return LearningEngine.GenerateLearningItems(recordList);
            }
            return 0;
        }

        public void NumberOfItems_SelectionChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.Items.Count == 0)
                return;
            int itemsCount = Convert.ToInt32(cb.SelectedItem);
            this.IsStartEnabled = itemsCount > 0;
            LearningEngine.ItemsCount = itemsCount;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

    }
}

