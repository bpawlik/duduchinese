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
        public bool IsStartEnabled { get; set; } = false;
        private List<RevisionItem> revisionList = null;

        public RevisePageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }

            this.Items = new ObservableCollection<string>();
            this.SelectedItemsCount = new ObservableCollection<string>();
        }

        string _Value = "Blah";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        private static string AllLists = "All";

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }

            // Restore shell
            Views.Shell.HamburgerMenu.IsFullScreen = false;

            this.IsStartEnabled = false;
            this.SelectedItemsCount.Clear();

            // Load revisions
            RevisionEngine.Deserialize();

            // Reset learning engine
            LearningEngine.Reset();

            // Update lists
            App app = (App)Application.Current;
            this.Items.Clear();
            this.Items.Add(AllLists);
            foreach (string key in app.ListManager.Keys)
                this.Items.Add(key);

            // Update items count combobox
            this.revisionList = RevisionEngine.RevisionList;
            SelectedItemsCount.Clear();
            for (int i = 10; i < this.revisionList.Count; i += 10)
                SelectedItemsCount.Add(i.ToString());
            SelectedItemsCount.Add(this.revisionList.Count.ToString());

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

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        private int NumberOfItems { get; set; } = 0;

        public void SelectedListChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            App app = (App)Application.Current;
            if (cb.SelectedValue != null)
            {
                string name = cb.SelectedValue.ToString();
                this.revisionList = RevisionEngine.GetRevisionList(-1, name == AllLists ? null : name);

                // Update items count combobox
                SelectedItemsCount.Clear();
                for (int i = 10; i < this.revisionList.Count; i += 10)
                    SelectedItemsCount.Add(i.ToString());
                SelectedItemsCount.Add(this.revisionList.Count.ToString());
            }
        }

        public void NumberOfItems_SelectionChanged(object sender)
        {
            ComboBox cb = sender as ComboBox;
            this.NumberOfItems = Convert.ToInt32(cb.SelectedValue);
            if (this.NumberOfItems > 0)
                this.IsStartEnabled = true;
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
                LearningEngine.UpdateRevisionList(this.revisionList.GetRange(0, this.NumberOfItems));
                LearningEngine.Mode = LearningMode.Revision;
                NavigationService.Navigate(typeof(Views.ProgressPage), 0);
            }
        }

    }
}

