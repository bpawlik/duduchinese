using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DuDuChinese.Converters
{
    public class BooleanColorConverter : IValueConverter
    {
        public SolidColorBrush True { get; set; }
        public SolidColorBrush False { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (bool)value ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotSupportedException();
        }
    }
}
