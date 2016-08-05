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

namespace DuDuChinese.ViewModels
{
    public class ListPageViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; private set; }

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

            //// TODO:
            //Items = DictionaryManager.GetList(Title);
            //Words = Items.Words;

            App app = (App)Application.Current;
            DictionaryRecordList list = app.ListManager[Title];

            // Do I really need to recreate it ?
            bool trad = false;
            foreach (DictionaryRecord r in list)
            {
                // determine what Hanzi to show to the user
                string chinese = (!trad || r.Chinese.Simplified.Equals(r.Chinese.Traditional))
                    ? r.Chinese.Simplified                                                     // show only simplified
                    : String.Format("{0} ({1})", r.Chinese.Simplified, r.Chinese.Traditional); // else "simple (trad)"

                this.Items.Add(new ItemViewModel()
                {
                    Record = r,
                    Pinyin = r.Chinese.Pinyin,
                    English = String.Join("; ", r.English),
                    EnglishWithNewlines = String.Join("\n", r.English),
                    Chinese = chinese,
                    Index = r.Index
                });
            }


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

        //public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        //{
        //    //Title = (suspensionState.ContainsKey(nameof(Title))) ? suspensionState[nameof(Title)]?.ToString() : parameter?.ToString();
        //    ProgressItems = (suspensionState.ContainsKey(nameof(Title))) ? (DictionaryItemList)suspensionState[nameof(Title)] : (DictionaryItemList)parameter;
        //    Title = ProgressItems.Title;
        //    Words = ProgressItems.Words;

        //    // Time to load the list

        //    await Task.CompletedTask;
        //}

        //public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        //{
        //    if (suspending)
        //    {
        //        suspensionState[nameof(Title)] = Title;
        //    }
        //    await Task.CompletedTask;
        //}

        //public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        //{
        //    args.Cancel = false;
        //    await Task.CompletedTask;
        //}
    }
}

