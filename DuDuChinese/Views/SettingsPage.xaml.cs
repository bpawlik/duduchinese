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
using Windows.Storage;
using System.IO.Compression;
using System.Diagnostics;

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

            // Update latest backup filename

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string localFilename = "";
            if (localSettings.Values.ContainsKey("latestBackupFile"))
                localFilename = (string)localSettings.Values["latestBackupFile"];

            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string remoteFilename = "";
            if (roamingSettings.Values.ContainsKey("latestBackupFile"))
                remoteFilename = (string)roamingSettings.Values["latestBackupFile"];

            UpdateBackupVersion(localFilename, remoteFilename);
        }

        private async void backupButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Just to be sure that revisions are loaded
            await RevisionEngine.Deserialize();

            // Save folder picker settings
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Zip file", new List<string>() { ".zip" });
            DateTime time = DateTime.Now;
            picker.SuggestedFileName = "DuDuChinese_" + time.ToString("yyyy-MM-dd-HHmm");

            // Pick a file
            Windows.Storage.StorageFile zipFile = await picker.PickSaveFileAsync();
            if (zipFile == null)
            {
                BackupStatus("Backup cancelled.", false);
                return;
            }

            // Create temporary folder with current date
            Windows.Storage.StorageFolder temporaryRootFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            Windows.Storage.StorageFolder temporaryFolder = await temporaryRootFolder.CreateFolderAsync(
                picker.SuggestedFileName,
                Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            if (temporaryFolder == null)
            {
                BackupStatus("Backup cancelled. Folder could not be created.", false);
                return;
            }

            // Dump revisions
            try
            {
                Windows.Storage.StorageFile revisionsFile = await temporaryFolder.CreateFileAsync(
                    revisionsFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (Stream stream = await revisionsFile.OpenStreamForWriteAsync())
                    RevisionEngine.Serialize(stream);
                BackupStatus("Revisions saved successfully.");
            }
            catch (Exception ex)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to save revisions to the file: {0}", revisionsFilename));
                messageDialog.Title = "Save Revision List Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to save revisions.", false);
                Debug.WriteLine(String.Format("Failed to save revisions: {0}", ex.Message));
                return;
            }

            // Save lists
            try
            {
                App app = (App)Application.Current;
                app.ListManager.SaveAll(temporaryFolder);
                BackupStatus("Backup successful.");
            }
            catch (Exception ex)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to save lists to the folder: {0}", temporaryFolder.Path));
                messageDialog.Title = "Save Lists Error";
                await messageDialog.ShowAsync();
                BackupStatus("Failed to backup lists.", false);
                Debug.WriteLine(String.Format("Failed to backup lists: {0}", ex.Message));
                return;
            }

            /// Zip files
            try
            {
                // Read files to compress
                IReadOnlyList<StorageFile> filesToCompress = await temporaryFolder.GetFilesAsync();

                // Create stream to compress files in memory (ZipArchive can't stream to an IRandomAccessStream
                using (MemoryStream zipMemoryStream = new MemoryStream())
                {
                    // Create zip archive
                    using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
                    {
                        // For each file to compress...
                        foreach (StorageFile fileToCompress in filesToCompress)
                        {
                            // ...read the contents of the file
                            byte[] buffer = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(
                                await FileIO.ReadBufferAsync(fileToCompress));

                            // Create a zip archive entry
                            ZipArchiveEntry entry = zipArchive.CreateEntry(fileToCompress.Name);

                            // And write the contents to it
                            using (Stream entryStream = entry.Open())
                            {
                                await entryStream.WriteAsync(buffer, 0, buffer.Length);
                            }
                        }
                    }

                    using (Windows.Storage.Streams.IRandomAccessStream zipStream = await zipFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // Write compressed data from memory to file
                        using (Stream outstream = zipStream.AsStreamForWrite())
                        {
                            byte[] buffer = zipMemoryStream.ToArray();
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BackupStatus("Failed to backup files.", false);
                Debug.WriteLine(String.Format("Failed to backup files: {0}", ex.Message));
                return;
            }

            BackupStatus("Backup successful.");
            UpdateSettingsData(zipFile.Name);
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private async void restoreButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Load folder picker settings
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".zip");

            // Pick a folder
            Windows.Storage.StorageFile zipFile = await picker.PickSingleFileAsync();
            if (zipFile == null)
            {
                BackupStatus("Restoring cancelled.", false);
                return;
            }

            // Ask for confirmation
            var yesCommand = new Windows.UI.Popups.UICommand("Yes");
            var noCommand = new Windows.UI.Popups.UICommand("No");
            var yesNoDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Are you sure you want to load revisions and lists from the file: {0}?\n\n" +
                                  "This action will overwrite existing revisions and lists.", zipFile.Name));
            yesNoDialog.Commands.Add(yesCommand);
            yesNoDialog.Commands.Add(noCommand);
            var result = await yesNoDialog.ShowAsync();
            if (result == noCommand)
            {
                BackupStatus("Restoring backup cancelled.", false);
                return;
            }

            // Unzip files
            Stream zipMemoryStream = await zipFile.OpenStreamForReadAsync();
            StorageFolder unzipFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFolder temporaryFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            App app = (App)Application.Current;

            // Create zip archive to access compressed files in memory stream
            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read))
            {
                // Unzip compressed file iteratively.
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (entry.FullName == revisionsFilename)
                    {
                        // Restore revisions
                        try
                        {
                            using (Stream entryStream = entry.Open())
                            {
                                byte[] buffer = new byte[entry.Length];
                                entryStream.Read(buffer, 0, buffer.Length);

                                // Create a file to store the revision list
                                StorageFile uncompressedFile = await unzipFolder.CreateFileAsync(revisionsFilename, CreationCollisionOption.ReplaceExisting);

                                // Store the content
                                using (Windows.Storage.Streams.IRandomAccessStream uncompressedFileStream =
                                    await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                                    {
                                        outstream.Write(buffer, 0, buffer.Length);
                                        outstream.Flush();
                                    }
                                }
                                await RevisionEngine.Deserialize();
                            }

                            // Save new revisions
                            RevisionEngine.Serialize();
                            BackupStatus("Revisions loaded successfully.");
                        }
                        catch
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(
                                String.Format("Failed to load revisions from the file: {0}. Is {1} missing?", zipFile.Path, revisionsFilename));
                            messageDialog.Title = "Restore Error";
                            await messageDialog.ShowAsync();
                            BackupStatus("Failed to restore revision list.", false);
                            return;
                        }
                    }
                    else
                    {
                        // Load lists
                        try
                        {
                            using (Stream entryStream = entry.Open())
                            {
                                byte[] buffer = new byte[entry.Length];
                                entryStream.Read(buffer, 0, buffer.Length);

                                // Create temporary file to store a list
                                StorageFile uncompressedFile = await temporaryFolder.CreateFileAsync(entry.FullName, CreationCollisionOption.ReplaceExisting);

                                // Save list to the temporary file
                                using (Windows.Storage.Streams.IRandomAccessStream uncompressedFileStream =
                                    await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                                    {
                                        outstream.Write(buffer, 0, buffer.Length);
                                        outstream.Flush();
                                    }
                                }

                                // Load list from the temporary file
                                using (var stream = await uncompressedFile.OpenStreamForReadAsync())
                                    app.ListManager.LoadListFromStream(entry.FullName, stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(
                                String.Format("Failed to load lists from the file: {0}\n\nError: {1}", zipFile.Path, ex.Message));
                            messageDialog.Title = "Load Lists Error";
                            await messageDialog.ShowAsync();
                            BackupStatus("Failed to restore lists.", false);
                            return;
                        }
                    }
                }
            }
            BackupStatus("Restoring successful.");
            UpdateSettingsData(zipFile.Name);
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void BackupStatus(string text, bool success = true)
        {
            backupStatus.Text = text;
            backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(success ? Windows.UI.Colors.Green : Windows.UI.Colors.Red);
        }
        private string GetLastDeviceUsed()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            return (string)roamingSettings.Values["lastDeviceUsed"];
        }


        private void UpdateSettingsData(string filename)
        {
            UpdateLocalData(filename);
            UpdateRoamingData(filename);
            UpdateBackupVersion(filename, filename);
        }

        private void UpdateLocalData(string filename)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["latestBackupFile"] = filename;
        }

        private void UpdateRoamingData(string filename)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["latestBackupFile"] = filename;
        }

        private void UpdateBackupVersion(string localVersion, string remoteVersion)
        {
            Windows.UI.Xaml.Media.Brush defaultTextColor = 
                (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark) ?
                new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White) :
                new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);

            if (localVersion == remoteVersion)
            {
                this.backupVersion.Text = "Current version: \t" + localVersion.Replace(".zip", "");
                this.backupVersion.Foreground = defaultTextColor;
            }
            else
            {
                this.backupVersion.Text = String.Format("Local version: \t{0}\nRemote version: \t{1}", localVersion, remoteVersion);
                this.backupVersion.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }

            this.backupVersion.Text += "\nLast device used: \t" + GetLastDeviceUsed();
        }
    }
}
