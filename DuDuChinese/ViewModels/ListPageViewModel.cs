using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using DuDuChinese.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using CC_CEDICT.Universal;
using Windows.UI.Xaml.Media;
using Windows.Media.SpeechSynthesis;

namespace DuDuChinese.ViewModels
{
    public class ListPageViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; private set; }

        public MediaElement Media { get; internal set; }

        public DictionaryRecord SelectedItem { get; set; } = null;

        DictionaryRecordList list;

        //public ObservableCollection<DictionaryItem> Words { get; private set; } = new ObservableCollection<DictionaryItem>();
        //public DictionaryItemList Items { get; set; }
        private string title = "Default";
        public string Title { get { return title; } set { Set(ref title, value); } }

        public ListPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Title = "Empty list";
            }

            this.Items = new ObservableCollection<ItemViewModel>();
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Title = (suspensionState.ContainsKey(nameof(Title))) ? suspensionState[nameof(Title)]?.ToString() : parameter?.ToString();

            App app = (App)Application.Current;
            list = app.ListManager[Title];

            LoadListData();

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Title)] = Title;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        void LoadListData()
        {
            List<DictionaryRecord> data = new List<DictionaryRecord>(list);
            switch (list.SortOrder)
            {
                case DictionaryRecordList.ListSortOrder.ReverseChronological:
                    data.Reverse();
                    break;
                case DictionaryRecordList.ListSortOrder.Alphabetical:
                default:
                    data.Sort();
                    break;
            }
            LoadData(data);
        }

        public void LoadData(List<DictionaryRecord> items)
        {
            ClearData();
            //this.NetworkAvailable = NetworkInterface.GetIsNetworkAvailable();

            foreach (DictionaryRecord r in items)
            {
                this.Items.Add(new ItemViewModel()
                {
                    Record = r,
                    Pinyin = r.Chinese.Pinyin,
                    English = String.Join("; ", r.English),
                    EnglishWithNewlines = String.Join("\n", r.English),
                    Chinese = r.Chinese.Simplified,
                    Index = r.Index
                });
            }
        }

        public void ClearData()
        {
            this.Items.Clear();
        }

        public async void Play()
        {
            // If the media is playing, the user has pressed the button to stop the playback.
            if (Media.CurrentState.Equals(MediaElementState.Playing))
            {
                Media.Stop();

                // Wait for a while
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                // Try again
                if (Media.CurrentState.Equals(MediaElementState.Playing))
                    return;
            }

            string text = SelectedItem.Chinese.Simplified;

            if (!String.IsNullOrEmpty(text))
            {
                try
                {
                    // Create a stream from the text. This will be played using a media element.
                    App app = (App)Application.Current;
                    SpeechSynthesisStream synthesisStream = await app.Synthesizer.SynthesizeTextToStreamAsync(text);

                    // Set the source and start playing the synthesized audio stream.
                    Media.AutoPlay = true;
                    Media.SetSource(synthesisStream, synthesisStream.ContentType);
                    Media.Play();
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
                    Media.AutoPlay = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "Unable to synthesize text. Please download Chinese simplified speech language pack.");
                    await messageDialog.ShowAsync();
                }
            }
        }

        public void Sort()
        {
            switch (list.SortOrder)
            {
                case DictionaryRecordList.ListSortOrder.Alphabetical:
                    list.SortOrder = DictionaryRecordList.ListSortOrder.ReverseChronological;
                    break;
                case DictionaryRecordList.ListSortOrder.ReverseChronological:
                    list.SortOrder = DictionaryRecordList.ListSortOrder.Alphabetical;
                    break;
            }
            LoadListData(); // reload
        }

        public void CopyToClipboard()
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(SelectedItem.Chinese.Simplified);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }

        public void Delete()
        {
            list.Remove(SelectedItem);
            LoadListData();
        }

        public void Search()
        {
            // NavigationService.Navigate is actually serializing the object.
            // To avoid making DictionarRecord serializable, it's better to
            // use SessionState dicitonary to pass objects over different pages.
            SessionState.Clear();
            SessionState.Add("Search", SelectedItem);
            NavigationService.Navigate(typeof(Views.MainPage), "Search");
        }

        public void Decompose()
        {
            SessionState.Clear();
            SessionState.Add("Decompose", SelectedItem);
            NavigationService.Navigate(typeof(Views.MainPage), "Decompose");
        }
    }
}

