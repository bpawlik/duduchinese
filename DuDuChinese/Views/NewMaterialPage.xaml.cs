using System;
using DuDuChinese.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace DuDuChinese.Views
{
    public sealed partial class NewMaterialPage : Page
    {
        public NewMaterialPage()
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

        private void NumberOfItems_DropDownOpened(object sender, object e)
        {
            // Select 10 by default
            if (itemsCountComboBox.Items.Count > 0)
                itemsCountComboBox.SelectedIndex = 0;
        }
    }
}
