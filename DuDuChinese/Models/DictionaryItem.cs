using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuDuChinese.Models
{
    enum State
    {
        Traditonal = 0,
        Simplified = 1,
        Pinyin = 2,
        Translation = 3
    }

    /// <summary>
    /// Dictionary item basic format is:
    /// Traditional Simplified [pin1 yin1] /English equivalent 1/equivalent 2/
    /// </summary>
    public class DictionaryItem
    {
        string traditional = String.Empty;
        string simplified = String.Empty;
        List<string> pinyin = null;
        List<string> translation = null;
        string oneLine = String.Empty;

        public string Traditional
        {
            get { return this.traditional; }
        }

        public string Simplified
        {
            get { return this.simplified; }
        }

        public List<string> Pinyin
        {
            get { return this.pinyin; }
        }

        public List<string> Translation
        {
            get { return this.translation; }
        }

        public string OneLine
        {
            get { return this.oneLine; }
        }

        public DictionaryItem(string line)
        {
            oneLine = line;
            State state = State.Traditonal;

            StringBuilder sb = new StringBuilder();

            int idx = 0;
            string translationTemp = "";

            foreach (char c in line)
            {
                switch (state)
                {
                    case State.Traditonal:
                        if (c == ' ')
                        {
                            state = State.Simplified;
                        }
                        else
                        {
                            traditional += c;
                        }

                        break;
                    case State.Simplified:
                        if (c == ' ')
                        {
                            state = State.Pinyin;
                        }
                        else
                        {
                            simplified += c;
                        }

                        break;
                    case State.Pinyin:
                        if (c == '[')
                        {
                            pinyin = new List<string>();
                            pinyin.Add(String.Empty);
                            continue;
                        }
                        else if (c == ']')
                        {
                            state = State.Translation;
                            idx = 0;
                        }
                        else if (c == ' ')
                        {
                            pinyin.Add(String.Empty);
                            idx++;
                            continue;
                        }
                        else
                        {
                            pinyin[idx] += c;
                        }

                        break;
                    case State.Translation:
                        translationTemp += c;
                        break;
                }
            }

            translation = new List<string>(translationTemp.Split('/'));
        }
    }
}
