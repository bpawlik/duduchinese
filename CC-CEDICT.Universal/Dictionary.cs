using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;

namespace CC_CEDICT.Universal
{
    public class Dictionary : StreamLineArray<DictionaryRecord>
    {
        public Dictionary<string, string> Header = new Dictionary<string, string>();

        public Dictionary(string path)
        {
            //ReadFile(path);

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                Read(new IsolatedStorageFileStream(path, FileMode.Open, store));
        }

        public Dictionary(Stream stream)
        {
            Read(stream);
        }

        private async void ReadFile(string path)
        {
            StorageFolder store = ApplicationData.Current.LocalFolder;
            string folderName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            StorageFolder listStorage = null;
            if (String.IsNullOrWhiteSpace(folderName))
                listStorage = store;
            else
                listStorage = await store.GetFolderAsync(folderName);
            if (File.Exists(Path.Combine(listStorage.Path, fileName)))
            {
                StorageFile file = await listStorage.GetFileAsync(fileName);
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    if (stream.CanRead)
                    {
                        Read(stream);
                    }
                }
            }

            await Task.CompletedTask;
        }

        protected override bool ProcessHeader(ref byte[] data, int offset, int length)
        {
            if (data[offset] != (byte)'#')
                return false;

            if (data[offset + 1] == (byte)'!' && data[offset + 2] == (byte)' ')
            {
                string header = System.Text.Encoding.UTF8.GetString(data, offset + 3, length - 3);
                int i = header.IndexOf("=");
                string key = header.Substring(0, i);
                string value = header.Substring(i + 1);
                switch (key)
                {
                    //case ...
                    default:
                        Header[key] = value;
                        break;
                }
            }

            return true;
        }
    }
}
