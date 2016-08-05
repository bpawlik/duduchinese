using System;
using System.Collections.Generic;
using Template10.Mvvm;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace DuDuChinese.ViewModels
{
    public class ListViewModel : ViewModelBase
    {
        public ObservableCollection<ListItemViewModel> Items { get; private set; }
        public ListViewModel()
        {
            this.Items = new ObservableCollection<ListItemViewModel>();
        }

        public void LoadData()
        {
            this.Items.Clear();
            
            App app = (App)Application.Current;
            List<string> lists = new List<string>();
            foreach (string key in app.ListManager.Keys)
                lists.Add(key);
            lists.Sort();
            foreach (string name in lists)
            {
                DictionaryRecordList list = app.ListManager[name];
                string lineTwo = list.Count.ToString();
                Items.Add(new ListItemViewModel { Name = list.Name, LineTwo = lineTwo, IsDeleted = list.IsDeleted });
            }
        }

        private bool _IsActive = false;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if (value != _IsActive)
                    Set(ref _IsActive, value);
            }
        }

        private bool _AddInProgress = false;
        public bool AddInProgress
        {
            get
            {
                return _AddInProgress;
            }
            set
            {
                if (value != _AddInProgress)
                    Set(ref _AddInProgress, value);
            }
        }

        private bool _EditInProgress = false;
        public bool EditInProgress
        {
            get
            {
                return _EditInProgress;
            }
            set
            {
                if (value != _EditInProgress)
                    Set(ref _EditInProgress, value);
            }
        }

        /// <summary>
        /// Inverse of Add/EditInProgress for XAML binding.
        /// </summary>
        public bool NotBusy
        {
            get
            {
                return (!AddInProgress && !EditInProgress);
            }
        }
    }
}
