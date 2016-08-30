using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using DuDuChinese.Models;

namespace DuDuChinese.Views
{
    public sealed partial class ExerciseTextBoxPage : Page
    {
        public ExerciseTextBoxPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            ViewModel.Media = media;
        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.Continue_Click(sender, e);
                this.continueButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.TextBox_TextChanged((sender as Windows.UI.Xaml.Controls.TextBox).Text);
        }

        private void Continue_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Continue_Click(sender, e);
            this.inputTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            Bindings.Update();
        }

        private void Accept_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Accept_Click(sender, e);
            this.inputTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            Bindings.Update();
        }

        private void Pinyin_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            CC_CEDICT.Universal.DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            p.Colorize(textBlock, record);
        }

        private void PlayButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Play();
        }

        private void PlaySentence_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Play(sentence: true);
        }
    }
}

