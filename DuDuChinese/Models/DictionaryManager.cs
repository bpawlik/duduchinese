using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DuDuChinese.Models;
using System.Runtime.Serialization;
using Windows.Storage;
using System.Xml;

namespace DuDuChinese
{
    [DataContract]
    public class DictionaryManager
    {
        static List<DictionaryItem> dictionary = null;
        static Dictionary<string, DictionaryItemList> lists = null;

        [DataMember]
        public static List<DictionaryItem> Dictionary
        {
            get
            {
                if (dictionary == null)
                    dictionary = new List<DictionaryItem>();
                return dictionary;
            }
        }

        [DataMember]
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

        public static async void Serialize()
        {
            // Get the local folder.
            StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new file named data_file.xml
            StorageFile file = await data_folder.CreateFileAsync("blah.xml", CreationCollisionOption.ReplaceExisting);

            // Write the data
            using (Stream s = await file.OpenStreamForWriteAsync())
            {
                //var ms = new MemoryStream();
                // Write an object to the Stream and leave it opened
                using (var writer = XmlDictionaryWriter.CreateTextWriter(s, Encoding.UTF8, ownsStream: false))
                {
                    var ser = new DataContractSerializer(typeof(Dictionary<string, DictionaryItemList>));
                    ser.WriteObject(writer, Lists);
                }
            }
        }

        public static async void Deserialize()
        {
            try
            {
                // Get the local folder.
                StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

                if (!File.Exists(Path.Combine(data_folder.Path, "blah.xml")))
                    return;

                // Create a new file named data_file.xml
                StorageFile file = await data_folder.GetFileAsync("blah.xml");

                // Write the data
                using (Stream s = await file.OpenStreamForReadAsync())
                {
                    // Read Stream to the Serializer and Deserialize and close it
                    using (var reader = XmlDictionaryReader.CreateTextReader(s, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null))
                    {
                        var ser = new DataContractSerializer(typeof(Dictionary<string, DictionaryItemList>));
                        lists = (Dictionary<string, DictionaryItemList>)ser.ReadObject(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
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
