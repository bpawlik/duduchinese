using System;
using SevenZip.Compression.LZMA.Universal;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;

namespace DuDuChinese.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;
        private static readonly string revisionsFilename = "Revisions.xml";

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;
            backupStatus.Text = "";
        }

        private async void backupButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Just to be sure that revisions are loaded
            await RevisionEngine.Deserialize();

            // Save folder picker settings
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".dummy");  // dummy filetype needed to avoid crash

            // Pick a folder
            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null)
            {
                BackupStatus("Backup cancelled.", false);
                return;
            }

            // Create subfolder with current date
            Windows.Storage.StorageFolder destFolder = await folder.CreateFolderAsync(
                "DuDuChinese_" + System.DateTime.Now.ToString("yyyy-MM-dd"),
                Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            if (destFolder == null)
            {
                BackupStatus("Backup cancelled. Folder could not be created.", false);
                return;
            }

            // Dump revisions
            try
            {
                Windows.Storage.StorageFile revisionsFile = await destFolder.CreateFileAsync(
                    revisionsFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (Stream stream = await revisionsFile.OpenStreamForWriteAsync())
                    RevisionEngine.Serialize(stream);
                BackupStatus("Revisions saved successfully.");
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to save revisions to the file: {0}", revisionsFilename));
                messageDialog.Title = "Save Revision List Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to save revisions.", false);
            }

            // Save lists
            try
            {
                App app = (App)Application.Current;
                app.ListManager.SaveAll(destFolder);
                BackupStatus("Backup successful.");
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to save lists to the folder: {0}", destFolder.Path));
                messageDialog.Title = "Save Lists Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to backup files.", false);
            }
        }

        private async void restoreButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Load folder picker settings
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".dummy");  // dummy filetype needed to avoid crash

            // Pick a folder
            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null)
            {
                BackupStatus("Restoring cancelled.", false);
                return;
            }

            // Ask for confirmation
            var yesCommand = new Windows.UI.Popups.UICommand("Yes");
            var noCommand = new Windows.UI.Popups.UICommand("No");
            var yesNoDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format(@"Are you sure you want to load revisions and lists from the folder: {0}?
                                  This action will overwrite existing revisions and lists.", folder.Name));
            yesNoDialog.Commands.Add(yesCommand);
            yesNoDialog.Commands.Add(noCommand);
            var result = await yesNoDialog.ShowAsync();
            if (result == noCommand)
            {
                BackupStatus("Restoring backup cancelled.", false);
                return;
            }

            // Restore revisions
            try
            {
                Windows.Storage.StorageFile file = await folder.GetFileAsync(revisionsFilename);
                using (Stream stream = await file.OpenStreamForReadAsync())
                    RevisionEngine.Deserialize(stream);

                // Save new revisions
                RevisionEngine.Serialize();
                BackupStatus("Revisions loaded successfully.");
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to load revisions from the folder: {0}. Is {1} missing?", folder.Path, revisionsFilename));
                messageDialog.Title = "Restore Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to restore revision list.", false);
            }

            // Load lists
            try
            {
                App app = (App)Application.Current;
                IReadOnlyList<Windows.Storage.StorageFile> fileList = await folder.GetFilesAsync();
                foreach (var file in fileList)
                {
                    if (file.FileType != "list")
                        continue;
                    app.ListManager.LoadListFromFile(file.Name, folder.Path);
                }
                BackupStatus("Restoring successful.");
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to load lists from the folder: {0}", folder.Path));
                messageDialog.Title = "Load Lists Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to restore lists.", false);
            }
        }

        private void BackupStatus(string text, bool success = true)
        {
            backupStatus.Text = text;
            backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(success ? Windows.UI.Colors.Green : Windows.UI.Colors.Red);
        }
    }
}
