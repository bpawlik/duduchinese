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

        public List<string> SelectedItemsCount { get; set; } = new List<string>();

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
            this.SelectedItemsCount.Clear();

            // Reset learning engine
            LearningEngine.Reset();

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

        public void SelectedListChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            App app = (App)Application.Current;
            if (cb.SelectedValue != null)
            {
                DictionaryRecordList recordList = app.ListManager[cb.SelectedValue.ToString()];

                List<RevisionItem> learningItems = LearningEngine.GenerateLearningItems(recordList);
                List<RevisionItem> revisionItems = RevisionEngine.RevisionList;

                // Get the intersection of learningItems and revisionItems
                var intersectList = learningItems.Select(i => i).Intersect(revisionItems);

                // Remove those items from the list that overlap
                foreach (var item in intersectList)
                    recordList.Remove(recordList[item.Index]);

                LearningEngine.CurrentItemList = recordList;

                // Update items count combobox
                SelectedItemsCount.Clear();
                for (int i = 10; i < recordList.Count; i += 10)
                    SelectedItemsCount.Add(i.ToString());
                SelectedItemsCount.Add(recordList.Count.ToString());
            }
        }

        public void NumberOfItems_SelectionChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            int itemsCount = Convert.ToInt32(cb.SelectedValue);
            if (itemsCount > 0)
                this.IsStartEnabled = true;
            LearningEngine.ItemsCount = itemsCount;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

    }
}

