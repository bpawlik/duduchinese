using System;
using System.Collections.Generic;
using Template10.Mvvm;
using System.Collections.ObjectModel;

namespace DuDuChinese.ViewModels
{
    public class ListItemViewModel : ViewModelBase
    {
        private string _line1;
        public string Name
        {
            get
            {
                return _line1;
            }
            set
            {
                if (value != _line1)
                    Set(ref _line1, value);
            }
        }

        private string _line2;
        public string LineTwo
        {
            get
            {
                return _line2;
            }
            set
            {
                if (value != _line2)
                    Set(ref _line2, value);
            }
        }

        private string _line3;
        public string LineThree
        {
            get
            {
                return _line3;
            }
            set
            {
                if (value != _line3)
                    Set(ref _line3, value);
            }
        }

        private bool _isEditable = false;
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                if (value != _isEditable)
                    Set(ref _isEditable, value);
            }
        }

        private bool _isDeleted = false;
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (value != _isDeleted)
                    Set(ref _isDeleted, value);
            }
        }
    }
}
