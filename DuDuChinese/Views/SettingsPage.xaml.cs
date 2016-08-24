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

            // Save file picker settings
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeChoices.Add("XML file", new List<string>() { ".xml" });
            picker.SuggestedFileName = "DuDuChinese_Revisions_" + System.DateTime.Now.ToString("yyyy-MM-dd");

            // Pick a file
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                backupStatus.Text = "Saving revisions cancelled.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Dump revisions
            try
            {
                using (Stream stream = await file.OpenStreamForWriteAsync())
                    RevisionEngine.Serialize(stream);
                backupStatus.Text = "Revisions saved successfully.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to save revisions to the file: {0}", file.Name));
                messageDialog.Title = "Save Revision List Error";
                await messageDialog.ShowAsync();
                backupStatus.Text = "Failed to save revisions.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        private async void restoreButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Save file picker settings
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".xml");

            // Pick a file
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                backupStatus.Text = "Restoring revisions cancelled.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Ask for confirmation
            var yesCommand = new Windows.UI.Popups.UICommand("Yes");
            var noCommand = new Windows.UI.Popups.UICommand("No");
            var yesNoDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Are you sure you want to load revisions from the file: {0}?\n\nThis action will overwrite existing revisions.", file.Name));
            yesNoDialog.Commands.Add(yesCommand);
            yesNoDialog.Commands.Add(noCommand);
            var result = await yesNoDialog.ShowAsync();
            if (result == noCommand)
            {
                backupStatus.Text = "Restoring revisions cancelled.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Restore revisions
            try
            {
                using (Stream stream = await file.OpenStreamForWriteAsync())
                    RevisionEngine.Deserialize(stream);

                // Save new revisions
                RevisionEngine.Serialize();

                backupStatus.Text = "Revisions loaded successfully.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
            }
            catch
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format("Failed to load revisions from the file: {0}", file.Name));
                messageDialog.Title = "Restore Revision List Error";
                await messageDialog.ShowAsync();
                backupStatus.Text = "Failed to load revisions.";
                backupStatus.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }
        }
    }
}
