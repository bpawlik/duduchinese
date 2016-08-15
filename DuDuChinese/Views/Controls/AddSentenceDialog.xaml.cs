using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DuDuChinese.Views.Controls
{
    public sealed partial class AddSentenceDialog : ContentDialog
    {
        public static readonly DependencyProperty ChineseProperty = DependencyProperty.Register(
            "Chinese", typeof(string), typeof(AddSentenceDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty EnglishProperty = DependencyProperty.Register(
            "English", typeof(string), typeof(AddSentenceDialog), new PropertyMetadata(default(string)));

        public string Chinese
        {
            get { return (string)GetValue(ChineseProperty); }
            set { SetValue(ChineseProperty, value); }
        }

        public string English
        {
            get { return (string)GetValue(EnglishProperty); }
            set { SetValue(EnglishProperty, value); }
        }

        public AddSentenceDialog()
        {
            this.InitializeComponent();
            chineseTextBox.Text = "中文";
            englishTextBox.Text = "English";
            chineseTextBox.SelectAll();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Chinese = chineseTextBox.Text;
            this.English = englishTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
