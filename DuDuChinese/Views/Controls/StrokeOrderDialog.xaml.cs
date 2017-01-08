using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CC_CEDICT.Universal;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Storage;

namespace DuDuChinese.Views.Controls
{
    public sealed partial class StrokeOrderDialog : ContentDialog
    {
        private List<Chinese.Character> Characters { get; set; } = new List<Chinese.Character>();
        private static readonly string htmlID = "make-me-a-hanzi-animation-";
        private static readonly string cssID = "#" + htmlID;

        public StrokeOrderDialog(Chinese chinese)
        {
            this.InitializeComponent();

            foreach (var character in chinese.Characters)
                this.charactersListBox.Items.Add(character.Simplified.ToString());
            this.Characters = chinese.Characters;
            this.charactersListBox.SelectedIndex = 0;
        }

        private void charactersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSVG();
        }

        private void charactersListBox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            LoadSVG();
        }

        private async void LoadSVG()
        {
            // Load SVG
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFolder svgFolder = await localFolder.GetFolderAsync("SVGs");
            char character = this.Characters[this.charactersListBox.SelectedIndex].Simplified;
            string svgFile = String.Format("{0}.svg", (int)character);
            if (!File.Exists(Path.Combine(svgFolder.Path, svgFile)))
            {
                this.WebViewControl.NavigateToString("Animation not found!");
                return;
            }

            using (Stream stream = await svgFolder.OpenStreamForReadAsync(svgFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                string svg = reader.ReadToEnd();

                // Find the last '#make-me-a-hanzi-animation-' occurance and read out the index
                int startIdx = svg.LastIndexOf(cssID) + cssID.Length;
                string substr = svg.Substring(startIdx, 5);
                int endIdx = substr.IndexOf('{');
                int count = Convert.ToInt32(svg.Substring(startIdx, endIdx).Trim()) + 1;

                // Find first N paths and add IDs
                startIdx = 0;
                for (int i = 0; i < count; ++i)
                {
                    int insertIdx = svg.IndexOf("<path d", startIdx) + 6;
                    svg = svg.Insert(insertIdx, String.Format("id = \"{0}{1}\" ", htmlID, i));
                    startIdx += htmlID.Length;
                }

                this.WebViewControl.NavigateToString(svg);
            }
        }
    }
}
