using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DuDuChinese.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public Visibility True { get; set; }
        public Visibility False { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (bool)value ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return value != null ? value.Equals(True) : false;
        }
    }
}
