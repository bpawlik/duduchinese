using System;
using DuDuChinese.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using DuDuChinese.Models;

namespace DuDuChinese.Views
{
    public sealed partial class ListsPage : Page
    {
        public ListsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            Windows.UI.Xaml.Controls.Primitives.FlyoutBase flyoutBase = Windows.UI.Xaml.Controls.Primitives.FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        public void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            DictionaryItemList list = (DictionaryItemList)datacontext;


            //this datacontext is probably some object of some type T
        }

        public void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            //this datacontext is probably some object of some type T
        }

        private void listsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RenameList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
