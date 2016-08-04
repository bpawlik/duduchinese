using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System;

namespace DuDuChinese.Models
{
    public class VisualTreeHelperHelper
    {
        public static T FindFrameworkElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            try
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T && ((FrameworkElement)child).Name.Equals(name))
                        return (T)child;
                    var result = FindFrameworkElementByName<T>(child, name);
                    if (result != null)
                        return result;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
