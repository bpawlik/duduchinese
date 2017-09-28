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
        static Regex pattern6 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])$");
        static Regex pattern7 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)$");
        static Regex pattern8 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])$");
        static Regex pattern9 = new Regex("^([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)([1-5])([a-zA-Z]+)$");

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
                        first = false;
                        continue;
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
                        first = false;
                        continue;
                    }

                    Match match6 = pattern6.Match(word);
                    Match match7 = pattern7.Match(word);
                    if (match6.Success || match7.Success)
                    {
                        string w1 = match6.Success ?
                              (match6.Groups[1].ToString() + match6.Groups[2].ToString())
                            : (match7.Groups[1].ToString() + match7.Groups[2].ToString());
                        string w2 = match6.Success ?
                              (match6.Groups[3].ToString() + match6.Groups[4].ToString())
                            : (match7.Groups[3].ToString());
                        string w3 = match6.Success ?
                              (match6.Groups[5].ToString() + match6.Groups[6].ToString())
                            : (match7.Groups[5].ToString());

                        FillTextBlock(textBlock, w1, !first);
                        FillTextBlock(textBlock, w2, false);
                        FillTextBlock(textBlock, w3, false);
                        first = false;
                        continue;
                    }

                    Match match8 = pattern8.Match(word);
                    Match match9 = pattern9.Match(word);
                    if (match8.Success || match9.Success)
                    {
                        string w1 = match8.Success ?
                              (match8.Groups[1].ToString() + match8.Groups[2].ToString())
                            : (match9.Groups[1].ToString() + match9.Groups[2].ToString());
                        string w2 = match8.Success ?
                              (match8.Groups[3].ToString() + match8.Groups[4].ToString())
                            : (match9.Groups[3].ToString());
                        string w3 = match8.Success ?
                              (match8.Groups[5].ToString() + match8.Groups[6].ToString())
                            : (match9.Groups[5].ToString());
                        string w4 = match8.Success ?
                              (match8.Groups[7].ToString() + match8.Groups[8].ToString())
                            : (match9.Groups[7].ToString());

                        FillTextBlock(textBlock, w1, !first);
                        FillTextBlock(textBlock, w2, false);
                        FillTextBlock(textBlock, w3, false);
                        FillTextBlock(textBlock, w4, false);
                        first = false;
                    }
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
