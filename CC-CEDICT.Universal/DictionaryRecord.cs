using System;
using System.Collections.Generic;

namespace CC_CEDICT.Universal
{
    public class DictionaryRecord : ILine, IComparable, IEquatable<DictionaryRecord>
    {
        public Chinese Chinese = null;
        public List<string> English = new List<string>();
        public List<string> Sentence = new List<string>();
        public int Relevance = 100;

        #region ILine initialization

        public override void Initialize(ref byte[] data)
        {
            string line = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

            int i = 0;
            int j = line.IndexOf(" ", i);
            if (j == -1)
                return;
            string traditional = line.Substring(i, j - i);

            i = j + 1;
            j = line.IndexOf(" [", i);
            if (j == -1)
                return;
            string simplified = line.Substring(i, j - i);

            i = j + 2;
            j = line.IndexOf("] /", i);
            if (j == -1)
                return;
            string pinyin = line.Substring(i, j - i);

            try
            {
                Chinese = new Chinese(traditional, simplified, pinyin);
            }
            catch (Exception)
            {
                return;
            }

            i = j + 3;
            j = line.IndexOf("/", i);
            if (j == -1)
                return;
            English.Add(line.Substring(i, j - i));

            int k = line.IndexOf("//", i) - 1;
            if (k == -2)
                k = line.Length;

            while (k > j + 1)
            {
                i = j + 1;
                j = line.IndexOf("/", i);
                if (j == -1)
                    break;
                English.Add(line.Substring(i, j - i));
            }

            if (k == line.Length)
                return;

            // Parse sentence
            i = j + 4;
            k = line.IndexOf("//", i);
            if (k == -1)
                return;
            Sentence.Add(line.Substring(i, k - i));

            i = k + 2;
            k = line.IndexOf("//", i);
            if (k == -1)
            {
                Sentence.Clear();
                return;
            }
            Sentence.Add(line.Substring(i, k - i));
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0} {1} [{2}] /{3}/ //{4}//",
                Chinese.Traditional,
                Chinese.Simplified,
                Chinese.PinyinNoMarkup,
                String.Join("/", English),
                String.Join("//", Sentence));
        }

        #region IComparable interface

        // TODO-NTH: sort chinese by stroke count and popularity and in-order chinese (and deprioritise "variant of...")

        int IComparable.CompareTo(object obj)
        {
            DictionaryRecord other = (DictionaryRecord)obj;
            // compare first by relevance
            if (this.Relevance > other.Relevance)
                return -1;
            if (this.Relevance < other.Relevance)
                return 1;
            // then by Pinyin
            for (int i = 0; i < this.Chinese.Characters.Count; i++)
            {
                if (i >= other.Chinese.Characters.Count) // other is shorter than this
                    return 1;
                int cmp = this.Chinese.Characters[i].Pinyin.CompareTo(other.Chinese.Characters[i].Pinyin);
                if (cmp != 0)
                    return cmp;
            }
            if (this.Chinese.Characters.Count < other.Chinese.Characters.Count) // this is shorter than other
                return -1;
            // finally by English
            return String.Join(";", this.English).CompareTo(String.Join(";", other.English));
        }

        #endregion

        #region IEquatable interface

        public bool Equals(DictionaryRecord other)
        {
            return this.ToString().Equals(other.ToString());
        }

        #endregion
    }
}
