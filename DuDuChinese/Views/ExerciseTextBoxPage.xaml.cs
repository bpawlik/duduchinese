using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;

namespace DuDuChinese.Views
{
    public sealed partial class ExerciseTextBoxPage : Page
    {
        public ExerciseTextBoxPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.Continue_Click(sender, e);
                this.continueButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                Bindings.Update();
            }
        }

        private void Continue_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Continue_Click(sender, e);
            this.inputTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            Bindings.Update();
        }
    }
}

