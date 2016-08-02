using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

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
    [DataContract]
    public class DictionaryItem
    {

        string traditional = String.Empty;
        string simplified = String.Empty;
        List<string> pinyin = null;
        List<string> translation = null;
        string oneLine = String.Empty;

        [DataMember]
        public string Traditional
        {
            get { return this.traditional; }
            set { this.traditional = value; }
        }

        [DataMember]
        public string Simplified
        {
            get { return this.simplified; }
            set { this.simplified = value; }
        }

        [DataMember]
        public List<string> Pinyin
        {
            get { return this.pinyin; }
            set { this.pinyin = value; }
        }

        [DataMember]
        public List<string> Translation
        {
            get { return this.translation; }
            set { this.translation = value; }
        }

        [DataMember]
        public string OneLine
        {
            get { return this.oneLine; }
            set { this.oneLine = value; }
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

    public class StringFormatter : Windows.UI.Xaml.Data.IValueConverter
    {
        // This converts the value object to the string to display.
        // This will work with most simple types.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            List<string> list = (List<string>)value;
            string converterParameter = parameter as string;

            if (!string.IsNullOrEmpty(converterParameter))
            {

                return string.Join("\n", list.ToArray());
            }
            else
            {
                return string.Join(" ", list.ToArray());
            } 
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EntryToBackgroundConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string stringValue = value as string;

            if (string.IsNullOrEmpty(stringValue))
                return Windows.UI.Colors.Black;

            if (stringValue.EndsWith("1"))
                return Windows.UI.Colors.Red;
            else if (stringValue.EndsWith("2"))
                return Windows.UI.Colors.Orange;
            else if (stringValue.EndsWith("3"))
                return Windows.UI.Colors.Green;
            else if (stringValue.EndsWith("4"))
                return Windows.UI.Colors.Blue;

            return Windows.UI.Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
