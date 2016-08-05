using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DuDuChinese.Converters
{
    public class NotepadLookupConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            //if (DesignerProperties.IsInDesignTool)
            //    return true;
            App app = (App)Application.Current;
            bool availableList = false;
            foreach (DictionaryRecordList list in app.ListManager.Values)
                if (!list.ReadOnly && !list.IsDeleted)
                    availableList = true;
            return availableList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
