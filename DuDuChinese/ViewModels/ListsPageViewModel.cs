using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using DuDuChinese.Models;

namespace DuDuChinese.ViewModels
{
    public class ListsPageViewModel : ViewModelBase
    {
        int listCounter = 0;

        public ListsPageViewModel()
        {

            // Load contacts list
            //for (int i = 0; i < 100; i++)
            //{
            //    var item = new Contact()
            //    {
            //        LastName = string.Format("Task Title {0}", i)
            //    };
            //    this.Contacts.Add(item);
            //}


        }

        private DictionaryItemList selectedList;
        public ObservableCollection<DictionaryItemList> SavedLists { get; private set; } = new ObservableCollection<DictionaryItemList>();
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
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

        public async void UploadList()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".u8");
            picker.FileTypeFilter.Add(".msg");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                DictionaryItemList newItemList = new DictionaryItemList(file.Name, file);
                this.SavedLists.Add(newItemList);
                /*this.selectedList = newItemList;   */         }
            else
            {
                //this.fileTextBox.Text = "Operation cancelled.";
            }

        }

        public void AddList()
        {
            DictionaryItemList newItemList = new DictionaryItemList("New List " + listCounter.ToString());
            this.SavedLists.Add(newItemList);

            listCounter++;
        }

        // ItemClick event only happens when user is a Master state. In this state, 
        // selection mode is none and click event navigates to the details view.
        public void OnItemClick(object sender, ItemClickEventArgs e)
        {
            //// The clicked item it is the new selected contact
            //selectedList = e.ClickedItem as DictionaryItemList;

            //NavigationService.Navigate(typeof(Views.ListPage), selectedList);
            //if (PageSizeStatesGroup.CurrentState == NarrowState)
            //{
            //    // Go to the details page and display the item 
            //    Frame.Navigate(typeof(DetailsPage), selectedContact, new DrillInNavigationTransitionInfo());
            //}
            ////else
            //{
            //    // Play a refresh animation when the user switches detail items.
            //    //EnableContentTransitions();
            //}
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView currentListView = sender as ListView;

            selectedList = currentListView.SelectedItem as DictionaryItemList;

            NavigationService.Navigate(typeof(Views.ListPage), selectedList.Title);

            //if (PageSizeStatesGroup.CurrentState == WideState)
            //{
            //    if (MasterListView.SelectedItems.Count == 1)
            //    {
            //        selectedContact = MasterListView.SelectedItem as Contact;
            //        EnableContentTransitions();
            //    }
            //    // Entering in Extended selection
            //    else if (MasterListView.SelectedItems.Count > 1
            //         && MasterDetailsStatesGroup.CurrentState == MasterDetailsState)
            //    {
            //        VisualStateManager.GoToState(this, ExtendedSelectionState.Name, true);
            //    }
            //}
            //// Exiting Extended selection
            //if (MasterDetailsStatesGroup.CurrentState == ExtendedSelectionState &&
            //    MasterListView.SelectedItems.Count == 1)
            //{
            //    VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
            //}
        }

        public void GotoListPage() =>
            NavigationService.Navigate(typeof(Views.ListPage), selectedList);


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

