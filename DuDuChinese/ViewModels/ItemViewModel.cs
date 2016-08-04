using Template10.Mvvm;
using CC_CEDICT.Universal;

namespace DuDuChinese.ViewModels
{
    public class ItemViewModel : ViewModelBase
    {
        public DictionaryRecord Record;

        private string _pinyin;
        public string Pinyin
        {
            get
            {
                return _pinyin;
            }
            set
            {
                if (value != _pinyin)
                {
                    Set(ref this._pinyin, value);
                }
            }
        }

        private string _english;
        public string English
        {
            get
            {
                return _english;
            }
            set
            {
                if (value != _english)
                {
                    Set(ref this._english, value);
                }
            }
        }

        private string _englishWithNewlines;
        public string EnglishWithNewlines
        {
            get
            {
                return _englishWithNewlines;
            }
            set
            {
                if (value != _englishWithNewlines)
                {
                    Set(ref this._englishWithNewlines, value);
                }
            }
        }

        private string _chinese;
        public string Chinese
        {
            get
            {
                return _chinese;
            }
            set
            {
                if (value != _chinese)
                {
                    Set(ref this._chinese, value);
                }
            }
        }

        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value != _index)
                {
                    Set(ref this._index, value);
                }
            }
        }

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
