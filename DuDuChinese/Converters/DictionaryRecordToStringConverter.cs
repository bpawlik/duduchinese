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
            {
                DictionaryRecord record = (DictionaryRecord)value;
                string text = record.Chinese.Simplified;

                // Add pinyin if requested
                if (parameter is string && !String.IsNullOrEmpty(parameter as string))
                    text += " - " + record.Chinese.Pinyin;

                return text;
            }
            else
            {
                return "[]";
            }  
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
