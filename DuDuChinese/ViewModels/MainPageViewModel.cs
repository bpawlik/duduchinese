using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using CC_CEDICT.Universal;
using Windows.UI.Xaml;
using System.IO.IsolatedStorage;

namespace DuDuChinese.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public bool IsDataLoaded { get; set; } = false;
        public Searcher Searcher { get; set; }
        public Dictionary Dictionary { get; set; }
        public string LastQuery { get; set; } = "";
        public static string DefaultQueryText = "english, pin1yin1 or 中文";

        private string queryText = DefaultQueryText;
        public string QueryText
        {
            get { return this.queryText; }
            set { this.Set(ref this.queryText, value); }
        }

        private string statusText = "";
        public string StatusText
        {
            get { return this.statusText; }
            set { this.Set(ref this.statusText, value); }
        }

        private Visibility statusVisibility = Visibility.Collapsed;
        public Visibility StatusVisibility
        {
            get { return this.statusVisibility; }
            set { this.Set(ref this.statusVisibility, value); }
        }

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return this.selectedPivotIndex; }
            set { this.Set(ref this.selectedPivotIndex, value); }
        }

        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //Value = "Designtime value";
            }

            this.Items = new ObservableCollection<ItemViewModel>();
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
            }

            if (parameter is string && SessionState.ContainsKey(parameter as string))
            {
                DictionaryRecord record = (DictionaryRecord)SessionState[parameter as string];
                this.SelectedPivotIndex = 0;

                switch (parameter as string)
                {
                    case "Search":
                        Search(record);
                        break;
                    case "Decompose":
                        Decompose(record);
                        break;
                }
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(Value)] = Value;
            }
            this.ClearData();
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;

            await Task.CompletedTask;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

        public void GotoRevision() =>
            NavigationService.Navigate(typeof(Views.RevisePage));

        public void GotoNewMaterial() =>
            NavigationService.Navigate(typeof(Views.NewMaterialPage));

        public void GotoList(string name)
        {
            NavigationService.Navigate(typeof(Views.ListPage), name);
        }

        public void LoadDictionary()
        {
            if (!this.IsDataLoaded)
            {
                this.Dictionary = new Dictionary("cedict_ts.u8");
                this.Searcher = new Searcher(this.Dictionary, new Index("english.index"), new Index("pinyin.index"), new Index("hanzi.index"));
                this.StatusText = "Enter your search phrase above.";
                this.IsDataLoaded = true;
            }

            App app = (App)Application.Current;
            app.Dictionary = this.Dictionary;
        }

        public void RealizePreinstalledLists()
        {
            try
            {
                App app = (App)Application.Current;
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (string file in store.GetFileNames("*.list"))
                    {
                        Dictionary list = new Dictionary(file);
                        string name = list.Header[DictionaryRecordList.NameHeaderKey];
                        if (app.ListManager.ContainsKey(name))
                            continue;
                        foreach (DictionaryRecord r in list)
                            app.ListManager[name].Add(r);
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Couldn't load preinstalled lists!");
            }
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
            this.IsDataLoaded = true;
        }

        public void ClearData()
        {
            this.Items.Clear();
        }

        public void Search(int index)
        {
            DictionaryRecord record = this.Dictionary[index];
            Search(record);
        }

        public void Search(DictionaryRecord record)
        {
            this.QueryText = record.Chinese.Simplified;
            TriggerSearch(this.QueryText, 30);
        }

        public Dictionary<string, int> Prev { get; set; } = new Dictionary<string, int>();

        public void TriggerSearch(string query, int minRelevance)
        {
            List<DictionaryRecord> results = this.Searcher.Search(query, minRelevance);

            if (results.Count == 0 && this.Searcher.Total > 0) // try again
                results = this.Searcher.Search(query);

            // reset things that need to be reset :)
            this.Prev["Results"] = -1; // override expansion marker

            if (results.Count == 0)
            {
                this.StatusText = String.Format("No results for '{0}'. Try another search.", query);
                this.StatusVisibility = Visibility.Visible;
                ClearData();
            }
            else // replace old search results with new
            {
                this.StatusText = String.Format("Showing results for '{0}' (omitted '{1}')", this.Searcher.LastQuery, this.Searcher.Ignored);
                this.StatusVisibility = this.Searcher.SmartSearch ? Visibility.Visible : Visibility.Collapsed;
                LoadData(results);
            }

            this.LastQuery = query;

            // This would be good for mobile up to hide keyboard. On PC however is annoying.
            //Results.Focus(FocusState.Programmatic);
        }

        public void Decompose(DictionaryRecord record)
        {
            List<DictionaryRecord> results = new List<DictionaryRecord>();
            results.Add(record);
            foreach (Chinese.Character c in record.Chinese.Characters)
                results.AddRange(this.Searcher.Search(c.Simplified.ToString(), 100));
            QueryText = record.Chinese.Simplified + " (split)";
            this.Prev["Results"] = -1; // override expansion marker
            this.StatusVisibility = Visibility.Collapsed;
            LoadData(results);
        }

        public void CopyToClipboard(int index)
        {
            DictionaryRecord r = this.Dictionary[index];
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(r.Chinese.Simplified);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

        }
    }
}

