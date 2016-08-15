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
using System.Xml.Linq;
using Windows.ApplicationModel.Email;

namespace DuDuChinese.ViewModels
{
    public class ListPageViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public MediaElement Media { get; internal set; }
        public DictionaryRecord SelectedItem { get; set; } = null;
        private DictionaryRecordList list;

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

            foreach (DictionaryRecord r in items)
            {
                this.Items.Add(new ItemViewModel()
                {
                    Record = r,
                    Pinyin = r.Chinese.Pinyin,
                    English = String.Join("; ", r.English),
                    EnglishWithNewlines = String.Join("\n", r.English),
                    Chinese = r.Chinese.Simplified,
                    Index = r.Index,
                    Sentence = String.Join(" - ", r.Sentence)
                });
            }
        }

        public void ClearData()
        {
            this.Items.Clear();
        }

        public async void Play(bool sentence = false)
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

            string text = (sentence && SelectedItem.Sentence.Count > 0) ? SelectedItem.Sentence[0] : SelectedItem.Chinese.Simplified;

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

        public async void AddSentence()
        {
            var dialog = new DuDuChinese.Views.Controls.AddSentenceDialog(SelectedItem.Sentence);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string chinese = dialog.Chinese;
                string english = dialog.English;

                int idx = list.IndexOf(SelectedItem);
                if (idx == -1)
                    return;

                list[idx].Sentence.Clear();
                if (!String.IsNullOrWhiteSpace(chinese) && !String.IsNullOrWhiteSpace(english))
                    list[idx].Sentence.AddRange(new List<string>() { chinese, english });

                list.IsModified = true;
                LoadListData();

                // Create toast
                string message = list[idx].Sentence.Count == 2 ? "Sentence added for the list item: "
                    : "Sentence removed from the list item: ";
                var xmlDoc = CreateToast(message + list[idx].Chinese.Simplified);
                var toast = new Windows.UI.Notifications.ToastNotification(xmlDoc);
                toast.Tag = "AddSentence";
                var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
                notifi.Show(toast);

                // Wait for a while and remove toast
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(2000));
                Windows.UI.Notifications.ToastNotificationManager.History.Remove("AddSentence");
            }

            return;
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

        public static Windows.Data.Xml.Dom.XmlDocument CreateToast(string text = null)
        {
            var xDoc = new XDocument(
                new XElement("toast",
                    new XElement("visual",
                        new XElement("binding",
                            new XAttribute("template", "ToastGeneric"),
                            new XElement("text", "DuDuChinese"),
                            new XElement("text", text == null ? "Text copied to the clipboard" : text)
                            )
                        )
                    )
                );

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

        public async void CopyToClipboard()
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(SelectedItem.Chinese.Simplified);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

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

        public async void Email()
        {
            App app = (App)Application.Current;
            StringBuilder sb = new StringBuilder();
            StringBuilder s2 = new StringBuilder();

            foreach (ItemViewModel item in Items)
            {
                sb.AppendLine(item.Pinyin);
                sb.AppendLine(item.EnglishWithNewlines);
                sb.AppendLine(item.Chinese);
                sb.AppendLine();
                s2.AppendLine(item.Record.ToString());
            }

            sb.AppendLine("-- DuDuChinese Learning Dictionary -- https://github.com/bpawlik/duduchinese/wiki");
            sb.AppendLine("________________________________________");
            sb.AppendLine("CC-CEDICT ed. " + app.Dictionary.Header["date"]);
            sb.AppendLine();

            if (Encoding.UTF8.GetBytes(sb.ToString()).Length < 16384)
                sb.AppendLine(s2.ToString());

            sb.AppendLine("Redistributed under license. " + app.Dictionary.Header["license"]);

            try
            {
                EmailMessage email = new EmailMessage();
                email.Subject = String.Format("[Kuaishuo] {0}", list.Name);
                email.Body = sb.ToString();
                await EmailManager.ShowComposeNewEmailAsync(email);
            }
            catch (ArgumentOutOfRangeException)
            {
                int size = Encoding.UTF8.GetBytes(sb.ToString()).Length / 1024;
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    String.Format(
                    "Sorry, Windows Phone has a 64KB size limit for emails sent from applications. " +
                    "Your notepad contains too many items to email ({0}KB). Please remove some and try again.", size));
                await messageDialog.ShowAsync();
            }
        }
    }
}

