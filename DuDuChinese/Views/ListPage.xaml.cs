using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System;

namespace DuDuChinese.Views
{
    public sealed partial class ListPage : Page
    {
        public ListPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private void Pinyin_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            //ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            //DictionaryRecord record = item.Record;
            //PinyinColorizer p = new PinyinColorizer();
            //Settings settings = new Settings();
            //p.Colorize(textBlock, record, (PinyinColorScheme)settings.PinyinColorSetting);
        }

        int previous = -1;
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;

            if (previous != item)
            {
                ToggleView(list, item, true);
                if (previous != -1)
                    ToggleView(list, previous, false);
                previous = item;
            }

            list.SelectedIndex = -1; // reset
            list.ScrollIntoView(list.Items[item]);
        }

        void ToggleView(ListBox list, int index, bool expand)
        {
            ListBoxItem item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(index);
            if (item == null)
                return;

            StackPanel defaultView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "DefaultView");
            StackPanel expandedView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ExpandedView");
            StackPanel actionPanel = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ActionPanel");

            if (expand)
            {
                defaultView.Visibility = Visibility.Collapsed;
                expandedView.Visibility = Visibility.Visible;
                actionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                defaultView.Visibility = Visibility.Visible;
                expandedView.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
            }
        }
    }

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

