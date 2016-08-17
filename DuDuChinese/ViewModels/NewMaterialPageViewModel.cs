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
        public ObservableCollection<string> SelectedItemsCount { get; set; }
        public bool IsStartEnabled { get; set; } = false;
        private int NumberOfItems { get; set; } = 0;
        private DictionaryRecordList CurrentRecordList { get; set; } = null;

        public NewMaterialPageViewModel()
        {
            this.Items = new ObservableCollection<string>();
            this.SelectedItemsCount = new ObservableCollection<string>();
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Restore shell
            Views.Shell.HamburgerMenu.IsFullScreen = false;

            this.IsStartEnabled = false;
            this.SelectedItemsCount.Clear();

            // Load revisions
            RevisionEngine.Deserialize();

            // Reset learning engine
            LearningEngine.Reset();
            LearningEngine.Mode = LearningMode.Words;

            App app = (App)Application.Current;
            List<string> items = new List<string>();
            foreach (string key in app.ListManager.Keys)
                items.Add(key);
            items.Sort();

            this.Items.Clear();
            foreach (string val in items)
                this.Items.Add(val);

            UpdateItemsCount();

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
            LearningMode oldMode = LearningEngine.Mode;
            LearningEngine.Mode = rb.Tag == null ? LearningMode.Words : (LearningMode)Convert.ToInt32(rb.Tag);

            // Update items count combobox
            if (oldMode != LearningEngine.Mode && this.CurrentRecordList != null)
            {
                this.IsStartEnabled = false;
                this.NumberOfItems = LearningEngine.GenerateLearningItems(this.CurrentRecordList);

                UpdateItemsCount();
            }
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

        private void UpdateItemsCount()
        {
            SelectedItemsCount.Clear();
            for (int i = 10; i < this.NumberOfItems; i += 10)
                SelectedItemsCount.Add(i.ToString());
            SelectedItemsCount.Add(this.NumberOfItems.ToString());
        }

        public int SelectedListChanged(object sender)
        {
            this.IsStartEnabled = false;
            ComboBox cb = sender as ComboBox;
            App app = (App)Application.Current;
            if (cb.SelectedValue != null)
            {
                this.CurrentRecordList = app.ListManager[cb.SelectedValue.ToString()];
                this.NumberOfItems = LearningEngine.GenerateLearningItems(this.CurrentRecordList);

                // Update items count combobox
                UpdateItemsCount();
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

