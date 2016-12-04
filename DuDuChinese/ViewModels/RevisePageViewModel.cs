using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace DuDuChinese.ViewModels
{
    public class RevisePageViewModel : ViewModelBase
    {
        public ObservableCollection<string> Items { get; private set; }
        public ObservableCollection<string> SelectedItemsCount { get; set; }
        public ObservableCollection<LearningItem> RevisionItems { get; set; }
        public bool IsStartEnabled { get; set; } = false;
        private List<LearningItem> revisionList = null;
        private int NumberOfItems { get; set; } = 0;
        public bool IsDetailExpanded { get; set; } = false;
        public LearningItem SelectedItem { get; set; } = null;

        private string status = "";
        public string Status
        {
            get { return this.status; }
            set { this.Set(ref this.status, value); }
        }

        private string selectedListName = null;
        private string SelectedListName
        {
            get
            {
                if (this.selectedListName == AllLists)
                    return null;
                else
                    return this.selectedListName;
            }
            set
            {
                this.selectedListName = value;
            }
        }

        public RevisePageViewModel()
        {
            this.Items = new ObservableCollection<string>();
            this.SelectedItemsCount = new ObservableCollection<string>();
            this.RevisionItems = new ObservableCollection<LearningItem>();
        }

        private static string AllLists = "All";

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Restore shell
            Views.Shell.HamburgerMenu.IsFullScreen = false;

            this.IsStartEnabled = false;
            this.SelectedItemsCount.Clear();

            // Load revisions
            await RevisionEngine.Deserialize();

            // Reset learning engine
            LearningEngine.Reset();
            LearningEngine.Mode = LearningMode.Revision;

            // Update lists
            App app = (App)Application.Current;
            List<string> items = new List<string>();
            foreach (string key in app.ListManager.Keys)
                items.Add(key);
            items.Sort();

            this.Status = "";
            this.Items.Clear();
            this.Items.Add(AllLists);
            foreach (string val in items)
                this.Items.Add(val);

            // Select "All" list by default
            ComboBox listsComboBox = new ComboBox();
            listsComboBox.Items.Add(AllLists);
            listsComboBox.SelectedIndex = 0;
            SelectedListChanged(listsComboBox);

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

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        private void UpdateItemsCount()
        {
            if (this.revisionList == null || RevisionEngine.RevisionList.Count == 0)
            {
                this.Status = "Go to New Material page and learn something first! :)";
                return;
            }

            if (!String.IsNullOrEmpty(this.selectedListName) && this.revisionList.Count == 0)
            {
                if (this.selectedListName == AllLists)
                    this.Status = "No revisions for today!";
                else
                    this.Status = "No revisions for today for the list: " + this.selectedListName;
            }  

            SelectedItemsCount.Clear();
            for (int i = 10; i < this.revisionList.Count; i += 10)
                SelectedItemsCount.Add(i.ToString());
            SelectedItemsCount.Add(this.revisionList.Count.ToString());
        }

        public void SelectedListChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            App app = (App)Application.Current;
            if (cb.SelectedValue != null)
            {
                this.SelectedListName = cb.SelectedValue.ToString();
                this.revisionList = RevisionEngine.GetRevisionList(-1, this.SelectedListName);

                // Update items count combobox
                UpdateItemsCount();
                if (this.IsDetailExpanded)
                    UpdateDetails(toggle: false, fullList: true);
            }
        }

        public void NumberOfItems_SelectionChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            this.NumberOfItems = Convert.ToInt32(cb.SelectedValue);
            if (this.NumberOfItems > 0 && this.revisionList != null)
            {
                this.IsStartEnabled = true;
                if (this.NumberOfItems > this.revisionList.Count)
                    this.revisionList = RevisionEngine.GetRevisionList(-1, this.SelectedListName);

                if (this.NumberOfItems < this.revisionList.Count)
                {
                    // Shuffle list and limit number of elements
                    var shuffledList = new List<LearningItem>(this.revisionList.OrderBy(a => Guid.NewGuid()));
                    this.revisionList = shuffledList.GetRange(0, this.NumberOfItems);
                }
                    
            }

            if (this.IsDetailExpanded)
                UpdateDetails(toggle: false, fullList: false);
            LearningEngine.ItemsCount = this.NumberOfItems;
        }

        public async void Start_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (this.revisionList.Count == 0)
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
                LearningEngine.UpdateLearningList(this.revisionList);
                LearningEngine.Mode = LearningMode.Revision;
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
        }

        public void UpdateDetails(bool toggle = true, bool fullList = false)
        {
            if (this.revisionList == null)
                return;

            // Get full revision list for the current list
            List<LearningItem> detailRevisionList = RevisionEngine.GetRevisionList(-1, this.SelectedListName, fullList);

            if (toggle)
                this.IsDetailExpanded = !this.IsDetailExpanded;
            this.RevisionItems.Clear();

            if (!this.IsDetailExpanded)
                return;

            foreach (var item in detailRevisionList)
                this.RevisionItems.Add(item);
        }

        public void RemoveSelectedItem()
        {
            if (this.SelectedItem != null && this.revisionList != null)
            {
                this.revisionList.Remove(this.SelectedItem);
                RevisionEngine.RevisionList.Remove(this.SelectedItem);
                this.RevisionItems.Remove(this.SelectedItem);
                RevisionEngine.Serialize();
            }    
        }
    }
}

