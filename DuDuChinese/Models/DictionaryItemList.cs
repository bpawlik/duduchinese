using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DuDuChinese.Models
{
    public class DictionaryItemList
    {
        public DictionaryItemList(string title)
        {
            this.Title = title;
        }

        public DictionaryItemList(string title, Windows.Storage.StorageFile file)
        {
            this.Title = title;

            switch (file.FileType)
            {
                case "msg":
                    LoadDictionaryFromEmail(file);
                    break;
                default:
                    LoadDictionaryFromFile(file);
                    break;
            }
        }

        public string Title { get; set; }

        public ObservableCollection<DictionaryItem> Words { get; private set; } = new ObservableCollection<DictionaryItem>();

        public async void LoadDictionaryFromFile(Windows.Storage.StorageFile file)
        {
            try
            {
                // Get the file.
                System.IO.Stream file_stream = await file.OpenStreamForReadAsync();

                // Read the data.
                using (StreamReader streamReader = new StreamReader(file_stream))
                {
                    string line;
                    bool startFound = false;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (!startFound)
                        {
                            if (line.StartsWith("CC-CEDICT "))
                                startFound = true;
                            continue;
                        }

                        // If line starts with '#' then continue
                        if (line.StartsWith("#") || String.IsNullOrEmpty(line.Trim()))
                            continue;

                        this.Words.Add(new DictionaryItem(line));
                    }
                }

                //// Open the text file using a stream reader.
                //using (StreamReader sr = File.OpenText(file.Path))
                //{
                    
                //}
            }
            catch (Exception ex)
            {
                string oops = ex.Message;

            }

        }

        public void LoadDictionaryFromEmail(Windows.Storage.StorageFile file)
        {
            try
            {
                OutlookStorage.Message outlookMsg = new OutlookStorage.Message(file.Path);

                string bodyText = outlookMsg.BodyText;


                //// Open the text file using a stream reader.
                //using (StreamReader sr = File.OpenText(filename))
                //{
                //    //// Read the stream to a string, and write the string to the console.
                //    //String line = sr.ReadToEnd();

                //    string line;

                //    while ((line = sr.ReadLine()) != null)
                //    {
                //        // If line starts with '#' then continue
                //        if (line.StartsWith("#"))
                //            continue;

                //        this.Words.Add(new DictionaryItem(line));
                //    }
                //}
            }
            catch (Exception ex)
            {
                string oops = ex.Message;

            }

        }
    }
}
