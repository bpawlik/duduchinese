using System;
using DuDuChinese.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using CC_CEDICT.Universal;
using Windows.UI.Core;
using DuDuChinese.Models;
using SevenZip.Compression.LZMA.WindowsPhone;
using System.Reflection;
using System.IO;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Media.SpeechSynthesis;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml.Media;

namespace DuDuChinese.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            SearchPane.DataContext = ViewModel;

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> files = new List<string> {
                "cedict_ts.u8", "english.index", "hanzi.index", "pinyin.index", "hsklevel1.list", "hsklevel2.list" };
            foreach (string file in files)
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    if (!store.FileExists(file))
                        ExtractFile(file);

            if (inProgress == 0)
            {
                ViewModel.LoadDictionary();
                ViewModel.RealizePreinstalledLists();
                LoadLists();
            }  

            Query.Focus(FocusState.Programmatic);
        }

        //protected override void OnBackKeyPress(CancelEventArgs e)
        //{
        //    if (!ok)
        //    {
        //        MessageBox.Show("Please wait a few seconds until the dictionary and indexes are extracted.");
        //        e.Cancel = true;
        //    }
        //    else if (pivot.SelectedItem.Equals(ListsPane))
        //    {
        //        e.Cancel = true;
        //        pivot.SelectedItem = SearchPane;
        //    }

        //    base.OnBackKeyPress(e);
        //}

        //protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    App app = (App)Application.Current;
        //    switch (app.Transition)
        //    {
        //        case App.TransitionType.PostAdd: // after add to list, go back to search page
        //            //pivot.SelectedItem = SearchPane; // looks ugly
        //            break;
        //        case App.TransitionType.ListUpdate: // after delete, list page needs an update
        //            LoadLists();
        //            break;
        //        case App.TransitionType.Specialise: // from SearchButton_Click on lists page
        //            pivot.SelectedItem = SearchPane;
        //            Search(app.TransitionData);
        //            break;
        //        case App.TransitionType.Decompose: // from DecomposeButton_Click on lists page
        //            pivot.SelectedItem = SearchPane;
        //            Decompose(app.TransitionData);
        //            break;
        //        case App.TransitionType.MoveItem: // moving an item from one list to another
        //            RecordToAdd = app.TransitionData;
        //            pivot.SelectedItem = ListsPane;
        //            break;
        //        case App.TransitionType.None:
        //        default:
        //            break;
        //    }
        //    CleanupTransitionData(app);
        //    base.OnNavigatedTo(e);
        //}

        //void CleanupTransitionData(App app)
        //{
        //    app.Transition = App.TransitionType.None;
        //    app.TransitionData = null;
        //}

        #region decompress LZMA resources (dictionary, indexes, preinstalled lists)

        int inProgress = 0;
        void ExtractFile(string file)
        {
            inProgress++;
            IsolatedStorageDecoder decoder = new IsolatedStorageDecoder();
            IsolatedStorageDecoder.AllowConcurrentDecoding = false;
            decoder.ProgressChanged += new ProgressChangedEventHandler(decoder_ProgressChanged);
            decoder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(decoder_RunWorkerCompleted);
            Progress.Visibility = Visibility.Visible;
            Status.Visibility = Visibility.Visible;

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            Stream resourceStream = assembly.GetManifestResourceStream(assemblyName.Name + ".Assets." + file + ".lzma");

            if (resourceStream == null)
            {
                var items = assembly.GetManifestResourceNames();
                return;
            }

            decoder.DecodeAsync(resourceStream, file);
        }

        void decoder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string file = (string)e.UserState;
            Status.Text = String.Format("Extracting {0}... {1}%", file, e.ProgressPercentage);
            Progress.Value = e.ProgressPercentage;
        }

        private void decoder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (--inProgress > 0) // still busy
                return;
            Progress.Visibility = Visibility.Collapsed; // don't need this any more
            ViewModel.LoadDictionary();
            ViewModel.RealizePreinstalledLists();
        }

        #endregion

        #region load dictionary (and indexes)

       

        #endregion

        #region toggle search query placeholder text

        void Query_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Query.Text.Equals(MainPageViewModel.DefaultQueryText))
                ViewModel.QueryText = "";
            else
                Query.SelectAll();
            Bindings.Update();
        }

        void Query_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Query.Text.Length == 0)
                ViewModel.QueryText = MainPageViewModel.DefaultQueryText;
            Bindings.Update();
        }

        #endregion

        #region search

        void Query_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter || !ViewModel.IsDataLoaded)
                return;

            ViewModel.QueryText = Query.Text.Trim();
            if (ViewModel.QueryText.Length == 0)
            {
                ViewModel.StatusText = "";
                ViewModel.StatusVisibility = Visibility.Collapsed;
                ViewModel.ClearData();
                Results.Focus(FocusState.Programmatic);
                Bindings.Update();
                return;
            }

            int minRelevance = ViewModel.QueryText.Equals(ViewModel.LastQuery) ? 30 : 75;
            ViewModel.TriggerSearch(Query.Text, minRelevance);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            //DictionaryRecord record = ViewModel.Dictionary[i];
            //Search(record);
            ViewModel.Search(i);
        }

        //void Search(DictionaryRecord record)
        //{
        //    Query.Text = record.Chinese.Simplified;
        //    TriggerSearch(Query.Text, 30);
        //}

        #endregion

        #region pivot switching

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0: // Search page
                    if (RecordToAdd != null)
                        RecordToAdd = null; // cancel the incomplete add-to-list action
                    Query.Focus(FocusState.Programmatic);
                    if (ListsPane.DataContext != null && ListsPane.DataContext is ListViewModel)
                        (ListsPane.DataContext as ListViewModel).IsActive = false;
                    break;
                case 1:
                    if (ListsPane.DataContext != null && ListsPane.DataContext is ListViewModel)
                        (ListsPane.DataContext as ListViewModel).IsActive = false;
                    break;
                case 2: // List page
                    CreateDefaultList();
                    LoadLists();
                    (ListsPane.DataContext as ListViewModel).IsActive = true;
                    break;
            }
        }

        #endregion

        #region Chinese decomposition

        private void DecomposeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord record = ViewModel.Dictionary[i];
            Decompose(record);
        }

        void Decompose(DictionaryRecord record)
        {
            List<DictionaryRecord> results = new List<DictionaryRecord>();
            results.Add(record);
            foreach (Chinese.Character c in record.Chinese.Characters)
                results.AddRange(ViewModel.Searcher.Search(c.Simplified.ToString(), 100));
            Query.Text = record.Chinese.Simplified + " (split)";
            ViewModel.Prev[Results.Name] = -1; // override expansion marker
            Status.Visibility = Visibility.Collapsed;
            ViewModel.LoadData(results);
            Results.ScrollIntoView(Results.Items[0]);
        }

        #endregion

        #region Text-to-Speech button

        /// <summary>
        /// This is invoked when the user clicks on the play button.
        /// </summary>
        /// <param name="sender">Button that triggered this event</param>
        /// <param name="e">State information about the routed event</param>
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // If the media is playing, the user has pressed the button to stop the playback.
            if (media.CurrentState.Equals(MediaElementState.Playing))
            {
                media.Stop();
            }
            else
            {
                Button button = (Button)sender;
                int i;
                int.TryParse(button.Tag.ToString(), out i);
                string text = ViewModel.Dictionary[i].Chinese.Simplified;

                if (!String.IsNullOrEmpty(text))
                {
                    try
                    {
                        // Create a stream from the text. This will be played using a media element.
                        App app = (App)Application.Current;
                        SpeechSynthesisStream synthesisStream = await app.Synthesizer.SynthesizeTextToStreamAsync(text);

                        // Set the source and start playing the synthesized audio stream.
                        media.AutoPlay = true;
                        media.SetSource(synthesisStream, synthesisStream.ContentType);
                        media.Play();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                        // be able to start media playback. Handle this gracefully
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                        await messageDialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                        // If the text is unable to be synthesized, throw an error message to the user.
                        media.AutoPlay = false;
                        var messageDialog = new Windows.UI.Popups.MessageDialog(
                            "Unable to synthesize text. Please download Chinese simplified speech language pack.");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }

        #endregion

        #region Copy-to-Clipboard action

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord r = ViewModel.Dictionary[i];
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(r.Chinese.Simplified);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            //ToastPrompt toast = new ToastPrompt
            //{
            //    Title = "Copy",
            //    Message = "Text copied to clipboard",
            //    MillisecondsUntilHidden = 1000
            //};
            //toast.Show();
        }

        #endregion

        #region add-to-list action

        /// <summary>
        /// Defines the record to be added to a list. While non-null we are in "add-to-list" mode.
        /// </summary>
        DictionaryRecord RecordToAdd = null;
        public string TextNotification { get; set; } = "";

        private async void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i = int.Parse(button.Tag.ToString());
            DictionaryRecord record = ViewModel.Dictionary[i];
            App app = (App)Application.Current;

            // Find default list
            ListViewModel lvm = (ListViewModel)ListListBox.DataContext;
            ListItemViewModel defaultList = null;
            foreach (ListItemViewModel item in lvm.Items)
            {
                if (item.IsDefault)
                {
                    defaultList = item;
                    break;
                }
            }

            switch (app.ListManager.CountWriteable)
            {
                case 0:
                    //MessageBox.Show("You need to create a list before you can save items to it.");
                    return;
                case 1:
                    DictionaryRecordList list = app.ListManager.DefaultList();
                    list.Add(record);
                    //app.Transition = App.TransitionType.PostAdd;
                    //app.TransitionData = record;
                    OpenList(list.Name);
                    break;
                default:
                    if (defaultList != null)
                    {
                        DictionaryRecordList target = app.ListManager[defaultList.Name];
                        if (target.IsDeleted)
                            return;
                        target.Add(record);
                        LoadLists();
                        TextNotification = "Item added to the default list: " + defaultList.Name;

                        ContentDialog dialog = new ContentDialog();
                        dialog.Content = "Item added to the default list: " + defaultList.Name;
                        dialog.Title = "Success";
                        dialog.PrimaryButtonText = "OK";
                        dialog.CanDrag = true;

                        await dialog.ShowAsync();
                    }
                    else
                    {
                        RecordToAdd = record;
                        pivot.SelectedIndex = pivot.Items.IndexOf(ListsPane);
                    }

                    break;
            }
        }

        #endregion

        #region expand/collapse list items

        //Dictionary<string, int> prev = new Dictionary<string, int>();
        // IMPORTANT: this event handler is used by both search and notepad lists
        // DO NOT USE Results or NotepadItems objects directly
        // DO USE (ListBox)sender to ensure the correct list is manipulated
        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;

            int previous = ViewModel.Prev.ContainsKey(list.Name) ? ViewModel.Prev[list.Name] : -1;
            if (previous != item)
            {
                ToggleView(list, item, true);
                if (previous != -1)
                    ToggleView(list, previous, false);
                ViewModel.Prev[list.Name] = item;
            }

            list.SelectedIndex = -1; // reset
            list.ScrollIntoView(list.Items[item]);
        }

        //Brush old;
        void ToggleView(ListBox list, int index, bool expand)
        {
            ListBoxItem item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(index);
            if (item == null)
                return;

            StackPanel defaultView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "DefaultView");
            StackPanel expandedView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ExpandedView");
            StackPanel actionPanel = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ActionPanel");

            if (expand)
            {
                defaultView.Visibility = Visibility.Collapsed;
                expandedView.Visibility = Visibility.Visible;
                actionPanel.Visibility = Visibility.Visible;
                //old = item.BorderBrush;
                //item.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            }
            else
            {
                defaultView.Visibility = Visibility.Visible;
                expandedView.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
                //item.BorderBrush = old;
            }
        }

        #endregion

        #region list handling

        /// <summary>
        /// Create default "notepad" list (once).
        /// </summary>
        void CreateDefaultList()
        {
            //Settings s = new Settings();
            //if (s.NotepadCreatedSetting)
            //    return;

            //App app = (App)Application.Current;
            //DictionaryRecordList list = app.ListManager["notepad"];
            //s.NotepadCreatedSetting = true;

            //if (s.NotepadItemsSetting.Count > 0) // migrate old notepad to list
            //{
            //    bool PatienceMessageShown = false;
            //    DateTime start = DateTime.Now;
            //    foreach (int id in s.NotepadItemsSetting)
            //    {
            //        list.Add(d[id]);
            //        TimeSpan elapsed = DateTime.Now - start;
            //        if (!PatienceMessageShown && elapsed.TotalSeconds > 1)
            //        {
            //            MessageBox.Show(
            //                "Your notepad is being converted into a new style list. " +
            //                "This may take a few more seconds so please be patient. " +
            //                "Look for the 'notepad' list when the update completes.");
            //            PatienceMessageShown = true;
            //        }
            //    }

            //    s.NotepadItemsSetting.Clear(); // empty the old notepad so this doesn't happen twice
            //}
        }

        void LoadLists()
        {
            ListViewModel lvm = new ListViewModel();
            lvm.LoadData();
            ListsPane.DataContext = lvm;
            if (pivot.SelectedItem.Equals(ListsPane))
            {
                if (RecordToAdd != null)
                    lvm.AddInProgress = true;
            }
        }

        private void NewList_Click(object sender, EventArgs e)
        {
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            lvm.EditInProgress = true;
            ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "", IsEditable = true };
            lvm.Items.Insert(0, item);
            //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // disable "multi-add"
            ListListBox.ScrollIntoView(ListListBox.Items[0]);
        }

        private async void AppBarUploadButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".u8");
            picker.FileTypeFilter.Add(".msg");
            picker.FileTypeFilter.Add(".list");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Create new list
                ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
                string name = Path.GetFileNameWithoutExtension(file.Name);
                ListItemViewModel item = new ListItemViewModel { Name = name, LineTwo = "", IsEditable = true };
                lvm.Items.Insert(0, item);
                //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // disable "multi-add"
                ListListBox.ScrollIntoView(ListListBox.Items[0]);

                // Read file and add items to the list
                App app = (App)Application.Current;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    Dictionary list = new Dictionary(stream);
                    if (!app.ListManager.ContainsKey(name))
                        foreach (DictionaryRecord r in list)
                            if (r.Chinese != null)
                                app.ListManager[name].Add(r);
                }

                await System.Threading.Tasks.Task.CompletedTask;
            }
        }

        private void AppBarAddButton_Click(object sender, RoutedEventArgs e)
        {
            // If we are in the list view then add new list
            if (pivot.SelectedItem.Equals(ListsPane))
            {
                ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
                lvm.EditInProgress = true;
                ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "", IsEditable = true };
                lvm.Items.Insert(0, item);
                //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // disable "multi-add"
                ListListBox.ScrollIntoView(ListListBox.Items[0]);
            }
        }

        private async void SaveList_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ItemContainerGenerator.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeChoices.Add("List file", new List<string>() { ".list" });
            picker.SuggestedFileName = listItem.Name;

            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                // Read file and add items to the list
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    App app = (App)Application.Current;
                    app.ListManager.Save(stream, listItem.Name);
                }
            }
        }

        bool RenameListMode = false;
        string OldName;
        private void RenameList_Click(object sender, RoutedEventArgs e)
        {
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            lvm.EditInProgress = true;
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ItemContainerGenerator.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            OldName = listItem.Name;
            RenameListMode = true; // turn on editing mode
            listItem.IsEditable = true;
            lvm.IsActive = true;
        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ItemContainerGenerator.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            App app = (App)Application.Current;
            app.ListManager.Remove(listItem.Name);
            LoadLists();
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            lvm.IsActive = true;
        }

        private void ListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void ListItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void SetAsDefaultList_Click(object sender, RoutedEventArgs e)
        {
            // Iterate through all item list and remove default options
            ListViewModel lvm = (ListViewModel)ListListBox.DataContext;
            foreach (ListItemViewModel item in lvm.Items)
            {
                //FrameworkElement senderElement = item as FrameworkElement;
                //FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
                //bool isChecked = (bool)flyoutBase.ReadLocalValue(ToggleMenuFlyoutItem.IsCheckedProperty);

                //if (isChecked)
                //{
                //    flyoutBase.SetValue(ToggleMenuFlyoutItem.IsCheckedProperty, false);
                //}

                item.IsDefault = false;
            }

            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ItemContainerGenerator.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            listItem.IsDefault = true;

            //ToggleMenuFlyoutItem flyoutItem = sender as ToggleMenuFlyoutItem;
            //flyoutItem.IsChecked = true;
        }

        //private void DeleteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

        //    //this datacontext is probably some object of some type T
        //}

        void ListEdit_Loaded(object sender, RoutedEventArgs e)
        {
            ListEdit_VisibilityChanged(sender, null);
        }

        void ListEdit_VisibilityChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Visibility == Visibility.Collapsed)
                return;
            textBox.Select(textBox.Text.Length, 0); // position cursor at end
            textBox.Focus(FocusState.Programmatic);
        }

        private void ListEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            //ApplicationBar.IsVisible = false;
        }

        // LostFocus means "cancel"
        private void ListEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            ListViewModel lvm = (ListViewModel)ListListBox.DataContext;
            if (RenameListMode) // user was editing an existing list
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = OldName; // reset
                foreach (ListItemViewModel item in lvm.Items)
                    item.IsEditable = false;
            }
            else // user was creating a new list
            {
                LoadLists(); // to wipe out new/blank one
            }
            RenameListMode = false; // turn off renaming mode
            lvm.EditInProgress = false; // to reenable context menu
            if (pivot.SelectedItem.Equals(ListsPane)) // check they didn't pivot away
            {
                //ApplicationBar.IsVisible = true;
                //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true; // re-enable add list
            }
        }

        private void ListEdit_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            TextBox textBox = (TextBox)sender;
            string name = textBox.Text.Trim();
            if (name.Length == 0)
                return;
            App app = (App)Application.Current;
            if (name != OldName && app.ListManager.ContainsKey(name))
            {
                //MessageBox.Show(String.Format("There is already a list called '{0}'. Please choose another name.", name));
                return;
            }
            if (RenameListMode)
            {
                if (name != OldName)
                    app.ListManager.Rename(OldName, name);
            }
            else // create a new list
            {
                DictionaryRecordList list = app.ListManager[name];
            }
            LoadLists();
            ListListBox.UpdateLayout();
            int index = FindListItemIndexByName(ListListBox, name);
            ListListBox.ScrollIntoView(ListListBox.Items[index]);
        }

        private int FindListItemIndexByName(ListBox list, string name)
        {
            ListViewModel lvm = (ListViewModel)list.DataContext;
            int index = 0;
            foreach (ListItemViewModel item in lvm.Items)
            {
                if (item.Name.Equals(name))
                    return index;
                index++;
            }
            return -1;
        }

        private void ListListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            list.SelectedIndex = -1; // reset
            if (item == -1)
                return;

            
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            if (lvm.EditInProgress) // don't open lists while adding/renaming
                return;

            ListItemViewModel ivm = (ListItemViewModel)list.Items[item];
            if (RecordToAdd != null) // user is selecting a target list to add an entry
            {
                App app = (App)Application.Current;
                DictionaryRecordList target = app.ListManager[ivm.Name];
                if (target.IsDeleted)
                    return; // can't add items to a deleted list
                target.Add(RecordToAdd);
                //app.Transition = App.TransitionType.PostAdd;
                //app.TransitionData = RecordToAdd;
                RecordToAdd = null; // come out of add-to-list mode
                LoadLists();
                TextNotification = "Item added to the list: " + ivm.Name;
            }

            lvm.AddInProgress = false;
            OpenList(ivm.Name);
        }

        void OpenList(string name)
        {
            App app = (App)Application.Current;
            if (!app.ListManager.ContainsKey(name))
                return;

            if (app.ListManager[name].IsDeleted)
            {
                app.ListManager[name].IsDeleted = false; // opening restores a deleted list
                LoadLists(); // refresh to catch undelete
            }

            ViewModel.GotoList(name);
        }

        #endregion

        #region colour Pinyin

        private void Pinyin_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            p.Colorize(textBlock, record);
        }

        #endregion

        private void Settings_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
