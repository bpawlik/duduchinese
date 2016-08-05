using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DuDuChinese.Converters
{
    public class MinimumLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            string text = value.ToString();
            if (text.Contains(" "))
                text = text.Substring(0, text.IndexOf(" "));
            return text.Length > 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
