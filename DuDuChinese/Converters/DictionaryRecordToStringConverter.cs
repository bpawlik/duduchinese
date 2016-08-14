using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuDuChinese.Converters
{
    public class DictionaryRecordToStringConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value != null && value is DictionaryRecord)
                return ((DictionaryRecord)value).Chinese.Simplified;
            else
                return "[]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
