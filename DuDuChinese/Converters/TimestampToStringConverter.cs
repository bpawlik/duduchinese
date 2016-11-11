using System;

namespace DuDuChinese.Converters
{
    public class TimestampToStringConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value != null && value is DateTime)
                return ((DateTime)value).ToString("dd-MM-yyyy");
            else
                return "[]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
