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
using System.Xml.Linq;

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
                ViewModel.IsDataLoaded = true;
                Bindings.Update();
            }  

            Query.Focus(FocusState.Programmatic);
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

            ViewModel.IsDataLoaded = true;
            Bindings.Update();
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
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            ViewModel.Search(i);
        }

        #endregion

        #region Pivot switching

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0: // Search page
                    if (RecordToAdd != null)
                        RecordToAdd = null; // cancel the incomplete add-to-list action
                    Query.Focus(FocusState.Programmatic);
                    ViewModel.IsActive = false;
                    break;
                case 1:
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

        private void NewList_Click(object sender, EventArgs e)
        {
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            lvm.EditInProgress = true;
            ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "", IsEditable = true };
            lvm.Items.Insert(0, item);
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
                            name = message.Subject.Replace("[Kuaishuo] ", "");
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
                ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "", IsEditable = true };
                lvm.Items.Insert(0, item);
                //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // disable "multi-add"
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

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
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
                item.IsDefault = false;

            App app = (App)Application.Current;
            foreach (string key in app.ListManager.Keys)
                app.ListManager[key].IsDefault = false;

            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ListBoxItem lbItem = (ListBoxItem)ListListBox.ContainerFromItem(datacontext);
            ListItemViewModel listItem = (ListItemViewModel)lbItem.DataContext;

            app.ListManager[listItem.Name].IsDefault = true;
            listItem.IsDefault = true;
        }

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
        }

        private async void ListEdit_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
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

            // Update binding to get IsActive property updated and then focus back to the listbox
            Bindings.Update();
            ListListBox.Focus(FocusState.Programmatic);
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
    }
}
