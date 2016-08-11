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
            int count = ViewModel.SelectedListChanged(sender);

            // Update items count combobox
            itemsCountComboBox.Items.Clear();
            for (int i = 10; i < count; i += 10)
                itemsCountComboBox.Items.Add(i.ToString());
            itemsCountComboBox.Items.Add(count.ToString());

            Bindings.Update();
        }

        private void NumberOfItems_SelectionChanged(object sender, object e)
        {
            ViewModel.NumberOfItems_SelectionChanged(sender);
            Bindings.Update();
        }
    }
}
