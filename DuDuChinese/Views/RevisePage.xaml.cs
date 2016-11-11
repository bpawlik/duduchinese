using System;
using DuDuChinese.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace DuDuChinese.Views
{
    public sealed partial class RevisePage : Page
    {
        public RevisePage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void SelectedListChanged(object sender, object e)
        {
            ViewModel.SelectedListChanged(sender);
            Bindings.Update();
        }

        private void NumberOfItems_SelectionChanged(object sender, object e)
        {
            ViewModel.NumberOfItems_SelectionChanged(sender);
            Bindings.Update();
        }

        private void Details_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.UpdateDetails(toggle: true, fullList: true);
            Bindings.Update();
        }

        private void Remove_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.RemoveSelectedItem();
            Bindings.Update();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyToolkit.Controls.DataGrid dg = (MyToolkit.Controls.DataGrid)sender;
            ViewModel.SelectedItem = (Models.LearningItem)dg.SelectedItem;
        }
    }
}
