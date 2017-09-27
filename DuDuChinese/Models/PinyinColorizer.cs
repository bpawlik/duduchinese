using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CC_CEDICT.Universal;
using Windows.UI.Xaml.Documents;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace DuDuChinese.Models
{
    public class PinyinColorizer
    {
        public static Color DodgerBlue = Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF);
        public static Color LimeGreen = Color.FromArgb(0xFF, 0x32, 0xCD, 0x32);
        static readonly Color[] colors = { Colors.Black, Colors.Red, Colors.Orange, LimeGreen, DodgerBlue, Colors.Black };
        static Regex pattern1 = new Regex("^([a-zA-Z]+)([1-5])$");
        static Regex pattern2 = new Regex("^([a-zA-Z]+)$");
        static Regex pattern3 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])$");
        static Regex pattern4 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)$");
        static Regex pattern5 = new Regex("^([a-zA-Z1-5]+)$");

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

        public void ColorizeSentence(TextBlock textBlock, List<string> sentence)
        {
            if (sentence.Count < 2)
                return;

            textBlock.Text = "";
            bool first = true;
            string sentencePinyin = sentence.Count == 3 ? sentence[2] : null;

            // Add Chinese sentence first
            Run runCH = new Run();
            runCH.Text = sentence[0] + System.Environment.NewLine;
            textBlock.Inlines.Add(runCH);

            // Add Pinyin sentence
            if (!String.IsNullOrWhiteSpace(sentencePinyin))
            {
                string[] words = sentencePinyin.Replace(",", "").Replace(".", "").Replace("?", "").Replace("!", "").Split(' ');
                foreach (string word in words)
                {
                    Match match1 = pattern1.Match(word);
                    Match match2 = pattern2.Match(word);
                    if (match1.Success || match2.Success)
                    {
                        FillTextBlock(textBlock, word, !first);
                    }

                    Match match3 = pattern3.Match(word);
                    Match match4 = pattern4.Match(word);
                    if (match3.Success || match4.Success)
                    {
                        string w1 = match3.Success ?
                              (match3.Groups[1].ToString() + match3.Groups[2].ToString())
                            : (match4.Groups[1].ToString() + match4.Groups[2].ToString());
                        string w2 = match3.Success ?
                              (match3.Groups[3].ToString() + match3.Groups[4].ToString())
                            : (match4.Groups[3].ToString());

                        FillTextBlock(textBlock, w1, !first);
                        FillTextBlock(textBlock, w2, false);
                    }

                    first = false;
                }

                string lastChar = sentencePinyin[sentencePinyin.Length - 1].ToString();
                Match match5 = pattern5.Match(lastChar);
                if (match5.Success)
                    textBlock.Inlines.Add(new Run() { Text = Environment.NewLine });
                else
                    textBlock.Inlines.Add(new Run() { Text = lastChar + Environment.NewLine });
            }

            // Add english sentence
            Run runEN = new Run();
            runEN.Text = " - " + sentence[1];
            textBlock.Inlines.Add(runEN);
        }

        private void FillTextBlock(TextBlock textBlock, string pinyin, bool addSpaceBefore)
        {
            Run run = new Run();
            Pinyin p = new Pinyin(pinyin);
            run.Text = addSpaceBefore ? " " + p.MarkedUp : p.MarkedUp;
            Color color = colors[(int)p.Tone];
            if (color != Colors.Black) // black is special
                run.Foreground = new SolidColorBrush(color);
            textBlock.Inlines.Add(run);
        }
    }
}
