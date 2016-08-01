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
        List<DictionaryItem> dictionary = null;
        List<DictionaryItemList> lists = null;

        public List<DictionaryItem> Dictionary
        {
            get
            {
                return this.dictionary;
            }
        }

        public List<DictionaryItemList> Lists
        {
            get
            {
                return this.lists;
            }
        }

        public DictionaryManager()
        {
            this.dictionary = new List<DictionaryItem>();
        }

        public void LoadDictionary(string filename)
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

                        this.dictionary.Add(new DictionaryItem(line));
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
