using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace DuDuChinese.Converters
{
    public class VisibilityToSymbolConverter : IValueConverter
    {
        public Symbol AddSymbol = Symbol.Add;
        public Symbol EditSymbol = Symbol.Edit;

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return EditSymbol;
            else
                return AddSymbol;
            }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
