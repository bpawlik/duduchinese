using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using CC_CEDICT.Universal;

namespace DuDuChinese.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //Value = "Designtime value";
            }

            this.Items = new ObservableCollection<ItemViewModel>();
            //this.NetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

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

        public ObservableCollection<ItemViewModel> Items { get; private set; }

        public bool IsDataLoaded { get; private set; }

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

        //// TODO: enable play buttons for those where the audio has already been downloaded (even in no-network scenario)
        //public bool NetworkAvailable { get; private set; }
        //public void UpdateNetworkStatus(bool status)
        //{
        //    this.NetworkAvailable = status;
        //    NotifyPropertyChanged("NetworkAvailable");
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void NotifyPropertyChanged(String propertyName)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (null != handler)
        //    {
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}


    }
}

