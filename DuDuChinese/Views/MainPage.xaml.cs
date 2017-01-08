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
using SevenZip.Compression.LZMA.Universal;
using System.Reflection;
using System.IO;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Media.SpeechSynthesis;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml.Media;
using System.Xml.Linq;
using System.Linq;
using Windows.Storage;
using System.IO.Compression;

namespace DuDuChinese.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            SearchPane.DataContext = ViewModel;
            ViewModel.Media = media;

            // Add page loaded event
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            // Add remote data changed event
            Windows.Storage.ApplicationData.Current.DataChanged +=
                new Windows.Foundation.TypedEventHandler<Windows.Storage.ApplicationData, object>(DataChangeHandler);
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Unzip rest of the files
            List<string> files = new List<string> {
                "cedict_ts.u8", "english.index", "hanzi.index", "pinyin.index", "hsklevel1.list",
                "hsklevel2.list", "hsklevel3.list", "charactertraits.list", "travel.list" };
            foreach (string file in files)
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    if (!store.FileExists(file))
                        ExtractFile(file);

            // Unzip SVGs folder
            await UnzipSVGs();

            if (inProgress == 0)
            {
                ViewModel.LoadDictionary();
                ViewModel.RealizePreinstalledLists();
                LoadLists();
                RevisionEngine.Deserialize();
                ViewModel.IsDataLoaded = true;
                Bindings.Update();
            }  

            Query.Focus(FocusState.Programmatic);
            CheckDeviceUsed();
        }

        private async System.Threading.Tasks.Task UnzipSVGs()
        {
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // If folder SVGs already exists then return
            if (Directory.Exists(Path.Combine(localFolder.Path, "SVGs")))
                return;

            // Load zip file from resources
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            Stream zipMemoryStream = assembly.GetManifestResourceStream((assemblyName.Name + ".Assets.SVGs.zip"));
            StorageFolder svgFolder = await localFolder.CreateFolderAsync("SVGs");

            // Create zip archive to access compressed files in memory stream
            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read))
            {
                float counter = 0;
                float maxCount = zipArchive.Entries.Count;
                Progress.Visibility = Visibility.Visible;
                Status.Visibility = Visibility.Visible;

                // Unzip compressed file iteratively.
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    // Load SVGs file by file
                    try
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            byte[] buffer = new byte[entry.Length];
                            entryStream.Read(buffer, 0, buffer.Length);

                            // Create temporary file to store a list
                            StorageFile uncompressedFile = await svgFolder.CreateFileAsync(entry.FullName, CreationCollisionOption.ReplaceExisting);

                            // Save list to the file
                            using (Windows.Storage.Streams.IRandomAccessStream uncompressedFileStream =
                                await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                                {
                                    outstream.Write(buffer, 0, buffer.Length);
                                    outstream.Flush();
                                }
                            }
                        }
                        float percentage = 100.0f * ++counter / maxCount;
                        Status.Text = String.Format("Extracting animations: {0}... {1}%", entry.FullName, (int)percentage);
                        Progress.Value = percentage;
                    }
                    catch (Exception ex)
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog(
                            String.Format("Failed to load SVGs fom the resources.\n\nError: {1}", ex.Message));
                        messageDialog.Title = "Load SVGs Error";
                        await messageDialog.ShowAsync();
                        return;
                    }
                }
            }

            await System.Threading.Tasks.Task.CompletedTask;
            Progress.Visibility = Visibility.Collapsed;
        }

        async void CheckDeviceUsed()
        {
            // Get the last device used
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string lastDeviceUsed = (string)roamingSettings.Values["lastDeviceUsed"];

            // Get the current device name
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            var hostName = hostNames.FirstOrDefault(name => name.Type == Windows.Networking.HostNameType.DomainName)?.DisplayName ?? "???";

            // Update roaming data
            roamingSettings.Values["lastDeviceUsed"] = hostName;

            if (String.IsNullOrWhiteSpace(lastDeviceUsed))
                return;

            if (lastDeviceUsed != hostName)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    // Roaming data has changed, notify user
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        String.Format("DuDuChinese has been used on another device: {0}.\n\nYou might want to go to the Settings and sync the data", lastDeviceUsed));
                    messageDialog.Title = "Synchronization";
                    await messageDialog.ShowAsync();
                });
            }
        }

        async void DataChangeHandler(Windows.Storage.ApplicationData appData, object o)
        {
            // Get values from the roaming data
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string lastBackupFile = (string)roamingSettings.Values["latestBackupFile"];

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            async () =>
            {
                // Roaming data has changed, notify user
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Detected newer version of your backup data: {0}.\n\nGo to Settings if you'd like to load it.", lastBackupFile));
                messageDialog.Title = "Synchronization";
                await messageDialog.ShowAsync();
            });
        }

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
                return;

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

            ViewModel.IsDataLoaded = true;
            Bindings.Update();
        }

        #endregion

        #region Show stroke order

        private async void Character_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);

            ViewModel.ShowStrokeOrder(i);
        }

        #endregion

        #region Toggle search query placeholder text

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

        #region Search

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

            // If the input keyboard is visible then focus on the results to hide it
            Windows.UI.ViewManagement.InputPane inputPane = Windows.UI.ViewManagement.InputPane.GetForCurrentView();
            if (inputPane.OccludedRect.Width > 0)
                Results.Focus(FocusState.Programmatic);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            ViewModel.Search(i);
            Bindings.Update();
        }

        #endregion

        #region Pivot switching

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedPivotIndex = ((Pivot)sender).SelectedIndex;
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0: // Search page
                    if (RecordToAdd != null)
                        RecordToAdd = null; // cancel the incomplete add-to-list action
                    Query.Focus(FocusState.Programmatic);
                    ViewModel.IsActive = false;
                    break;
                case 1: // Learn page
                    ViewModel.IsActive = false;
                    break;
                case 2: // List page
                    ViewModel.RealizePreinstalledLists();
                    LoadLists();
                    ViewModel.IsActive = true;
                    break;
            }
            Bindings.Update();
        }

        #endregion

        #region Chinese decomposition

        private void DecomposeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord record = ViewModel.Dictionary[i];
            ViewModel.Decompose(record);
            Results.ScrollIntoView(Results.Items[0]);
            Bindings.Update();
        }

        #endregion

        #region Text-to-Speech button

        /// <summary>
        /// This is invoked when the user clicks on the play button.
        /// </summary>
        /// <param name="sender">Button that triggered this event</param>
        /// <param name="e">State information about the routed event</param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);

            ViewModel.Play(i);
        }

        #endregion

        #region Copy-to-Clipboard action

        public static Windows.Data.Xml.Dom.XmlDocument CreateToast(string text = null)
        {
            var xDoc = new XDocument(
                new XElement("toast",
                    new XElement("visual",
                        new XElement("binding",
                            new XAttribute("template", "ToastGeneric"),
                            new XElement("text", "DuDuChinese"),
                            new XElement("text", text == null ? 
                                "Text copied to the clipboard" : ("Item added to the list: " + text))
                            )
                        )
                    )
                );

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            ViewModel.CopyToClipboard(i);

            // Create toast
            var xmlDoc = CreateToast();
            var toast = new Windows.UI.Notifications.ToastNotification(xmlDoc);
            toast.Tag = "Clipboard";
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);

            // Wait for a while and remove toast
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(2000));
            Windows.UI.Notifications.ToastNotificationManager.History.Remove("Clipboard");
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
            ListItemViewModel defaultList = null;
            if (ListListBox.DataContext is ListViewModel)
            {
                ListViewModel lvm = (ListViewModel)ListListBox.DataContext;
                foreach (ListItemViewModel item in lvm.Items)
                {
                    if (item.IsDefault)
                    {
                        defaultList = item;
                        break;
                    }
                }
            }

            switch (app.ListManager.CountWriteable)
            {
                case 0:
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "You need to create a list before you can save items to it.");
                    await messageDialog.ShowAsync();
                    return;
                case 1:
                    DictionaryRecordList list = app.ListManager.DefaultList();
                    list.Add(record);
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

                        // Show toast
                        var xmlDoc = CreateToast(defaultList.Name);
                        var toast = new Windows.UI.Notifications.ToastNotification(xmlDoc);
                        toast.Tag = "AddToList";
                        var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
                        notifi.Show(toast);

                        // Wait for a while and remove toast
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(2000));
                        Windows.UI.Notifications.ToastNotificationManager.History.Remove("AddToList");
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
            ListBoxItem item = (ListBoxItem)list.ContainerFromIndex(index);
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

            IReadOnlyList<Windows.Storage.StorageFile> fileList = await picker.PickMultipleFilesAsync();
            foreach (var file in fileList)
            {
                if (file != null)
                {
                    // Create new list
                    ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
                    string name = Path.GetFileNameWithoutExtension(file.Name);
                    ListItemViewModel item = new ListItemViewModel { Name = name, LineTwo = "", IsEditable = false };
                    lvm.Items.Insert(0, item);
                    ListListBox.ScrollIntoView(ListListBox.Items[0]);

                    // Read file and add items to the list
                    App app = (App)Application.Current;
                    using (Stream stream = await file.OpenStreamForReadAsync())
                    {
                        Stream inStream = null;

                        if (file.FileType == ".msg")
                        {
                            OutlookStorage.Message message = new OutlookStorage.Message(stream);
                            MemoryStream memStream = new MemoryStream();
                            StreamWriter writer = new StreamWriter(memStream);

                            // Write header
                            name = message.Subject.Replace("[Kuaishuo] ", "").Replace("[DuDuChinese] ", "");
                            writer.Write("#! name=" + name + "\n#! readonly=False\n#! sortorder=0\n");

                            // Parse lines and add to the memory stream
                            string text = message.BodyText;
                            string[] lines = text.Split('\n');
                            bool isStart = false;
                            foreach (string line in lines)
                            {
                                if (line.Trim().StartsWith("CC-CEDICT ed."))
                                {
                                    isStart = true;
                                    continue;
                                }

                                if (!isStart || String.IsNullOrWhiteSpace(line))
                                    continue;

                                writer.WriteLine(line);
                            }
                            writer.Flush();
                            memStream.Position = 0;
                            inStream = memStream;
                        }
                        else if (file.FileType == ".txt")
                        {
                            MemoryStream memStream = new MemoryStream();
                            StreamWriter writer = new StreamWriter(memStream);

                            // Write header
                            writer.Write("#! name=" + name + "\n#! readonly=False\n#! sortorder=0\n");

                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                while (streamReader.Peek() >= 0)
                                {
                                    string line = streamReader.ReadLine();
                                    if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line))
                                        continue;
                                    writer.WriteLine(line);
                                }
                            }
                            writer.Flush();
                            memStream.Position = 0;
                            inStream = memStream;
                        }
                        else
                        {
                            inStream = stream;
                        }

                        Dictionary list = new Dictionary(inStream);
                        if (!app.ListManager.ContainsKey(name))
                            foreach (DictionaryRecord r in list)
                                if (r.Chinese != null)
                                    app.ListManager[name].Add(r);
                    }

                    await System.Threading.Tasks.Task.CompletedTask;
                }
            }

            LoadLists();
        }

        private void AppBarAddButton_Click(object sender, RoutedEventArgs e)
        {
            // If we are in the list view then add new list
            if (pivot.SelectedItem.Equals(ListsPane))
            {
                ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
                lvm.EditInProgress = true;
                ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "", IsEditable = false };
                lvm.Items.Insert(0, item);
                Bindings.Update();

                // Update binding and change editable mode in order to force focus on the control
                lvm.Items[0].IsEditable = true;
                Bindings.Update();

                ListListBox.ScrollIntoView(ListListBox.Items[0]);
            }
        }

        private async void SaveList_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
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
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            OldName = listItem.Name;
            RenameListMode = true; // turn on editing mode
            listItem.IsEditable = true;
            lvm.IsActive = true;
        }

        private List<string> listsToBeDeleted = new List<string>();
        private async void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            App app = (App)Application.Current;

            // Ask for confirmation
            var yesCommand = new Windows.UI.Popups.UICommand("Yes");
            var noCommand = new Windows.UI.Popups.UICommand("No");
            var yesNoDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Are you sure you want to delete list: {0}?", listItem.Name));
            yesNoDialog.Commands.Add(yesCommand);
            yesNoDialog.Commands.Add(noCommand);
            var result = await yesNoDialog.ShowAsync();
            if (result == noCommand)
                return;

            string filename = app.ListManager.Remove(listItem.Name);
            LoadLists();
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            lvm.IsActive = true;

            // Store the list for removal on the next launch
            Template10.Services.SettingsService.ISettingsHelper _helper = new Template10.Services.SettingsService.SettingsHelper();
            listsToBeDeleted.Add(filename);
            _helper.Write<List<string>>("ListsToBeRemoved", listsToBeDeleted);
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
            // Get the current value
            App app = (App)Application.Current;
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;
            bool currentValue = app.ListManager[listItem.Name].IsDefault;

            // Iterate through all item list and remove default options
            ListViewModel lvm = (ListViewModel)ListListBox.DataContext;
            foreach (ListItemViewModel item in lvm.Items)
                item.IsDefault = false;
            foreach (string key in app.ListManager.Keys)
                app.ListManager[key].IsDefault = false;

            // Toggle flag
            app.ListManager[listItem.Name].IsDefault = !currentValue;
            listItem.IsDefault = !currentValue;
            Bindings.Update();
        }

        void ListEdit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
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
        }

        private async void ListEdit_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            // Mark event as handled to avoid unintentional taps
            e.Handled = true;

            TextBox textBox = (TextBox)sender;
            string name = textBox.Text.Trim();
            if (name.Length == 0)
                return;

            App app = (App)Application.Current;
            if (name != OldName && app.ListManager.ContainsKey(name))
            {
                if (!RenameListMode)
                    return;

                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("There is already a list called '{0}'. Please choose another name.", name));
                await messageDialog.ShowAsync();
                return;
            }

            if (RenameListMode)
            {
                if (name != OldName)
                {
                    // Rename list in list manager
                    app.ListManager.Rename(OldName, name);

                    // Rename list in revision list
                    RevisionEngine.RenameList(OldName, name);
                }
            }
            else // create a new list
            {
                DictionaryRecordList list = app.ListManager[name];
            }

            LoadLists();
            ListListBox.UpdateLayout();
            int index = FindListItemIndexByName(ListListBox, name);
            ListListBox.ScrollIntoView(ListListBox.Items[index]);

            // Update binding to get IsActive property updated
            Bindings.Update();
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
            if (lvm.EditInProgress)
                return;

            // User is selecting a target list to add an entry
            ListItemViewModel ivm = (ListItemViewModel)list.Items[item];
            if (RecordToAdd != null) 
            {
                App app = (App)Application.Current;
                DictionaryRecordList target = app.ListManager[ivm.Name];
                if (target.IsDeleted)
                    return;

                // Amend index of the entry (important for learning)
                RecordToAdd.Index = target.Count;
                target.Add(RecordToAdd);

                // Reset and reload
                RecordToAdd = null;
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
    }
}
