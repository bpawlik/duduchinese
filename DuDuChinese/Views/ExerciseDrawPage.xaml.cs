using System;
using System.Linq;
using DuDuChinese.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using DuDuChinese.Models;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuDuChinese.Views
{
    public sealed partial class ExerciseDrawPage : Page
    {
        static readonly int CHARACTER_WIDTH = 300;

        public ExerciseDrawPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            ViewModel.Media = media;

            // Set supported inking device types.
            this.hanziCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Touch |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            // Set initial ink stroke attributes.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            drawingAttributes.PenTip = PenTipShape.Circle;
            drawingAttributes.Size = new Windows.Foundation.Size(15, 15);
            this.hanziCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

            DrawBackground();
        }

        private void DrawBackground(int gridCount = 1)
        {
            this.backgroundCanvas.Children.Clear();

            for (int i = 0; i < gridCount; ++i)
            {
                var line_NW_SE = new Line()
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.LightGray),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 1, 1 },
                    X1 = 0,
                    X2 = CHARACTER_WIDTH - 20,
                    Y1 = 0,
                    Y2 = CHARACTER_WIDTH - 20
                };
                Canvas.SetLeft(line_NW_SE, CHARACTER_WIDTH * i + 10);
                Canvas.SetTop(line_NW_SE, 10);
                this.backgroundCanvas.Children.Add(line_NW_SE);

                var line_NE_SW = new Line()
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.LightGray),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 1, 1 },
                    X1 = CHARACTER_WIDTH - 20,
                    X2 = 0,
                    Y1 = 0,
                    Y2 = CHARACTER_WIDTH - 20
                };
                Canvas.SetLeft(line_NE_SW, CHARACTER_WIDTH * i + 10);
                Canvas.SetTop(line_NE_SW, 10);
                this.backgroundCanvas.Children.Add(line_NE_SW);

                var line_N_S = new Line()
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.LightGray),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 1, 1 },
                    X1 = 0,
                    X2 = 0,
                    Y1 = 0,
                    Y2 = CHARACTER_WIDTH - 20
                };
                Canvas.SetLeft(line_N_S, CHARACTER_WIDTH * i + 150);
                Canvas.SetTop(line_N_S, 10);
                this.backgroundCanvas.Children.Add(line_N_S);

                var line_W_E = new Line()
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.LightGray),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 1, 1 },
                    X1 = 0,
                    X2 = CHARACTER_WIDTH - 20,
                    Y1 = 0,
                    Y2 = 0
                };
                Canvas.SetLeft(line_W_E, CHARACTER_WIDTH * i + 10);
                Canvas.SetTop(line_W_E, CHARACTER_WIDTH/2);
                this.backgroundCanvas.Children.Add(line_W_E);

                if (i > 0)
                {
                    var line_N_S_sep = new Line()
                    {
                        Stroke = new SolidColorBrush(Windows.UI.Colors.LightGray),
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection() { 1, 1 },
                        X1 = 0,
                        X2 = 0,
                        Y1 = 0,
                        Y2 = CHARACTER_WIDTH - 20
                    };
                    Canvas.SetLeft(line_N_S_sep, CHARACTER_WIDTH * i);
                    Canvas.SetTop(line_N_S_sep, 10);
                    this.backgroundCanvas.Children.Add(line_N_S_sep);
                }
            }
        }

        private async Task<string> RecognizeInput()
        {
            var inkRecognizer = new InkRecognizerContainer();

            int hanziIndex = 0;
            foreach (InkRecognizer inkR in inkRecognizer.GetRecognizers())
            {
                if (inkR.Name == "Microsoft 中文(简体)手写识别器")
                    break;
                hanziIndex++;
            }

            if (inkRecognizer != null)
            {
                // Set Hanzi recognizer
                inkRecognizer.SetDefaultRecognizer(inkRecognizer.GetRecognizers()[hanziIndex]);

                try
                {
                    var recognitionResults = await inkRecognizer.RecognizeAsync(
                        this.hanziCanvas.InkPresenter.StrokeContainer,
                        InkRecognitionTarget.All);

                    return string.Join("", recognitionResults.Select(i => i.GetTextCandidates()[0]));
                }
                catch { /* Do nothing */ }
            }
            return String.Empty;
        }

        private void Clear_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.hanziCanvas.InkPresenter.StrokeContainer.Clear();
        }

        private async void Continue_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.InputText = await RecognizeInput();

            if (ViewModel.Validated)
            {
                this.continueButton.Content = "Check";
                this.clearButton.Content = "Clear";
                this.clearButton.IsEnabled = true;
                Clear_Click(sender, e);
            }
            else
            {
                this.continueButton.Content = "Continue";
                this.clearButton.Content = "Retry";
                this.clearButton.IsEnabled = false;
            }
                
            ViewModel.Continue_Click(sender, e);
            Bindings.Update();
        }

        private void Accept_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Accept_Click(sender, e);
            Clear_Click(sender, e);
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

        private void Character_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.ShowStrokeOrder();
        }

        private void Hanzi_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int hanziCount = ViewModel.CurrentItem.Chinese.Characters.Count;
            this.hanziCanvas.Width = hanziCount * CHARACTER_WIDTH;
            this.backgroundCanvas.Width = hanziCount * CHARACTER_WIDTH;
            DrawBackground(hanziCount);

            this.continueButton.Content = "Check";
            this.clearButton.Content = "Clear";
            this.clearButton.IsEnabled = true;
        }

    }
}

