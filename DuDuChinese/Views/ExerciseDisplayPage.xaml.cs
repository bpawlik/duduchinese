using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using DuDuChinese.Models;
using System;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace DuDuChinese.Views
{
    public sealed partial class ExerciseDisplayPage : Page
    {
        public ExerciseDisplayPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            ViewModel.Media = media;
        }

        private void Pinyin_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            CC_CEDICT.Universal.DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            p.Colorize(textBlock, record);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Play();
        }

        private void PlaySentence_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Play(sentence: true);
        }

        private void Continue_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Continue_Click(sender, e);
            Bindings.Update();
        }
    }
}

