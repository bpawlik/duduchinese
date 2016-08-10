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

        private void AppBarAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Add sentence
        }

        private void AppBarSortButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Sort();
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
            //Button button = (Button)sender;
            //DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            //App app = (App)Application.Current;
            //app.Transition = App.TransitionType.Decompose;
            //app.TransitionData = record;
            //NavigationService.GoBack();
        }

        void SortButton_Click(object sender, EventArgs e)
        {
            ViewModel.Sort();
        }

        void EmailButton_Click(object sender, EventArgs e)
        {
            //StringBuilder sb = new StringBuilder();
            //StringBuilder s2 = new StringBuilder();

            //foreach (ItemViewModel item in NotepadItems.Items)
            //{
            //    sb.AppendLine(item.Pinyin);
            //    sb.AppendLine(item.EnglishWithNewlines);
            //    sb.AppendLine(item.Chinese);
            //    sb.AppendLine();
            //    s2.AppendLine(item.Record.ToString());
            //}

            //sb.AppendLine("-- Kuaishuo Chinese Dictionary http://www.knibb.co.uk/kuaishuo");
            //sb.AppendLine("________________________________________");
            //sb.AppendLine("CC-CEDICT ed. " + d.Header["date"]);
            //sb.AppendLine();

            //if (Encoding.UTF8.GetBytes(sb.ToString()).Length < 16384)
            //    sb.AppendLine(s2.ToString());

            //sb.AppendLine("Redistributed under license. " + d.Header["license"]);

            //try
            //{
            //    EmailComposeTask email = new EmailComposeTask();
            //    email.Subject = String.Format("[Kuaishuo] {0}", list.Name);
            //    email.Body = sb.ToString();
            //    email.Show();
            //}
            //catch (ArgumentOutOfRangeException)
            //{
            //    int size = Encoding.UTF8.GetBytes(sb.ToString()).Length / 1024;
            //    MessageBox.Show(String.Format(
            //        "Sorry, Windows Phone has a 64KB size limit for emails sent from applications. " +
            //        "Your notepad contains too many items to email ({0}KB). Please remove some and try again.", size));
            //}
        }
    }
}

