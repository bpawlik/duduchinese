using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DuDuChinese.Models;

namespace DuDuChinese
{
    public class DictionaryManager
    {
        static List<DictionaryItem> dictionary = null;
        static Dictionary<string, DictionaryItemList> lists = null;

        public static List<DictionaryItem> Dictionary
        {
            get
            {
                if (dictionary == null)
                    dictionary = new List<DictionaryItem>();
                return dictionary;
            }
        }

        public static Dictionary<string, DictionaryItemList> Lists
        {
            get
            {
                if (lists == null)
                    lists = new Dictionary<string, DictionaryItemList>();
                return lists;
            }
        }

        public static void AddList(DictionaryItemList list)
        {
            if (Lists.ContainsKey(list.Title))
                Lists[list.Title] = list;
            else
                Lists.Add(list.Title, list);
        }

        public static DictionaryItemList GetList(string title)
        {
            if (Lists.ContainsKey(title))
                return Lists[title];
            else
                return null;
        }

        public static void LoadDictionary(string filename)
        {
            try
            {
                // Open the text file using a stream reader.
                using (StreamReader sr = File.OpenText(filename))
                {
                    //// Read the stream to a string, and write the string to the console.
                    //String line = sr.ReadToEnd();

                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        // If line starts with '#' then continue
                        if (line.StartsWith("#"))
                            continue;

                        Dictionary.Add(new DictionaryItem(line));
                    }
                }
            }
            catch (Exception ex)
            {
                string oops = ex.Message;

            }
            
        }
    }
}
