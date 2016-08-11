using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System;
using DuDuChinese.Models;
using CC_CEDICT.Universal;

namespace DuDuChinese.Views
{
    public sealed partial class ListPage : Page
    {
        public ListPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            ViewModel.Media = media;
        }

        private void Pinyin_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            p.Colorize(textBlock, record);
        }

        int previous = -1;
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;

            ViewModel.SelectedItem = ((ItemViewModel)list.SelectedItem).Record;

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
            ListBoxItem item = (ListBoxItem)list.ContainerFromIndex(index);
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

        private void AppBarAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Add sentence
        }

        private void AppBarSortButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Sort();
        }

        private void AppBarEmailButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Email();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Play();
        }

        void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyToClipboard();
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete();

            previous = -1;
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Search();
        }

        void DecomposeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Decompose();
        }

        void SortButton_Click(object sender, EventArgs e)
        {
            ViewModel.Sort();
        }
    }
}

