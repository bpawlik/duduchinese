using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CC_CEDICT.Universal;
using Windows.UI.Xaml.Documents;

namespace DuDuChinese.Models
{
    public class PinyinColorizer
    {
        public static Color DodgerBlue = Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF);
        public static Color LimeGreen = Color.FromArgb(0xFF, 0x32, 0xCD, 0x32);
        static readonly Color[] colors = { Colors.Black, Colors.Red, Colors.Orange, LimeGreen, DodgerBlue, Colors.Black };

        public PinyinColorizer()
        {
        }

        public void Colorize(TextBlock textBlock, DictionaryRecord record)
        {
            textBlock.Text = "";
            bool first = true;

            foreach (Chinese.Character c in record.Chinese.Characters)
            {
                Run run = new Run();
                run.Text = first ? c.Pinyin.MarkedUp : " " + c.Pinyin.MarkedUp;
                Color color = colors[(int)c.Pinyin.Tone];
                if (color != Colors.Black) // black is special
                    run.Foreground = new SolidColorBrush(color);
                textBlock.Inlines.Add(run);
                first = false;
            }
        }
    }
}
