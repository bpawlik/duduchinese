using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DuDuChinese.Converters
{
    public class BooleanDoubleConverter : IValueConverter
    {
        public double True { get; set; }
        public double False { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (bool)value ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
