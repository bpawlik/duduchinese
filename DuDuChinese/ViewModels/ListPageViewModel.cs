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

namespace DuDuChinese.ViewModels
{
    public class ListPageViewModel : ViewModelBase
    {
        public ObservableCollection<DictionaryItem> Words { get; private set; } = new ObservableCollection<DictionaryItem>();
        public DictionaryItemList Items { get; set; }
        private string title = "Default";
        public string Title { get { return title; } set { Set(ref title, value); } }

        public ListPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Title = "Empty list";
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Title = (suspensionState.ContainsKey(nameof(Title))) ? suspensionState[nameof(Title)]?.ToString() : parameter?.ToString();

            // TODO:
            Items = DictionaryManager.GetList(Title);
            Words = Items.Words;

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
        //    Items = (suspensionState.ContainsKey(nameof(Title))) ? (DictionaryItemList)suspensionState[nameof(Title)] : (DictionaryItemList)parameter;
        //    Title = Items.Title;
        //    Words = Items.Words;

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

