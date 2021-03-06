﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CC_CEDICT.Universal;
using System.IO;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Threading;
using Windows.Storage;

namespace DuDuChinese
{
    public class ListManager : Dictionary<string, DictionaryRecordList>
    {
        /// <summary>
        /// Wraps DictionaryRecordList to add management functionality.
        /// </summary>
        public class ManagedList : DictionaryRecordList
        {
            public string SavePath;
            public string SaveFilename;
            int _Identifier = -1;

            public ManagedList(Dictionary dictionary)
                : base(dictionary)
            {
            }

            public ManagedList(string name)
                : base(name)
            {
            }

            /// <summary>
            /// The numeric part of the filename of this list.
            /// </summary>
            public int Identifier
            {
                get
                {
                    if (_Identifier < 0)
                        _Identifier = int.Parse(Path.GetFileNameWithoutExtension(this.SaveFilename));
                    return _Identifier;
                }
            }

            public async void Save(StorageFolder destinationFolder = null)
            {
                try
                {
                    if (IsDeleted || (destinationFolder == null && !IsModified)) // save not required
                        return;

                    Debug.WriteLine(String.Format("ManagedList.Save(): {0} ({1} entries)", SaveFilename, this.Count));
                    StorageFolder dataFolder = ApplicationData.Current.LocalFolder;
                    StorageFolder listsFolder = destinationFolder ?? await dataFolder.GetFolderAsync(_ListsDirectory);
                    StorageFile file = await listsFolder.CreateFileAsync(SaveFilename, CreationCollisionOption.ReplaceExisting);
                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(this.ToString());
                        stream.Write(data, 0, data.Length);
                    }

                    IsModified = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Couldn't save list: {0}", ex.Message));
                }
            }

            public void Save(Stream stream)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(this.ToString());
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Couldn't save list: {0}", ex.Message));
                }
            }

            public void Delete()
            {
                try
                {
                    if (!IsDeleted)
                        return;

                    Debug.WriteLine(String.Format("ManagedList.Delete(): {0}", SavePath));
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        if (store.FileExists(SavePath))
                            store.DeleteFile(SavePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Couldn't delete list: {0}", ex.Message));
                }
            }
        }

        Timer AutoSave;
        int MaxListIdentifier = -1;

        public ListManager()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (string file in store.GetFileNames(String.Format("{0}/*.list", ListsDirectory)))
                    {
                        LoadListFromFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't load lists: {0}", ex.Message);
            }

            // set up auto-save so that lists aren't only saved when the app shuts down
            AutoSave = new Timer((o) =>
            {
                foreach (ManagedList list in this.Values)
                    list.Save();
            }, null, 5000, 5000);
        }

        public void LoadListFromFile(string filename)
        {
            string path = String.Format("{0}/{1}", ListsDirectory, filename);
            Debug.WriteLine(String.Format("Loading list: {0}", path));
            Dictionary dictionary = new Dictionary(path);
            ManagedList list = new ManagedList(dictionary);
            list.SavePath = path;
            list.SaveFilename = filename;
            list.IsModified = true;
            if (base.ContainsKey(list.Name))
                base[list.Name] = list;
            else
                base.Add(list.Name, list);
            if (list.Identifier > MaxListIdentifier)
                MaxListIdentifier = list.Identifier;
        }

        public void LoadListFromStream(string filename, Stream stream)
        {
            Dictionary dictionary = new Dictionary(stream);
            ManagedList list = new ManagedList(dictionary);
            list.SavePath = String.Format("{0}/{1}", ListsDirectory, filename);
            list.SaveFilename = filename;
            list.IsModified = true;
            if (base.ContainsKey(list.Name))
                base[list.Name] = list;
            else
                base.Add(list.Name, list);
            if (list.Identifier > MaxListIdentifier)
                MaxListIdentifier = list.Identifier;
        }

        /// <summary>
        /// DictionaryRecordList accessor and auto-creator.
        /// </summary>
        /// <param name="name">Name of the list to retrieve or create.</param>
        /// <returns>The existing or newly-created DictionaryRecordList.</returns>
        public new DictionaryRecordList this[string name]
        {
            get
            {
                if (!this.ContainsKey(name)) // auto-create list
                {
                    ManagedList list = new ManagedList(name);
                    list.SaveFilename = String.Format("{0}.list", ++MaxListIdentifier);
                    list.SavePath = String.Format("{0}/{1}", ListsDirectory, list.SaveFilename);
                    base.Add(name, list);
                }
                return base[name];
            }
        }

        private new void Add(string key, DictionaryRecordList value)
        {
            throw new NotSupportedException("ListManager.Add(...) is not supported. Use ListManager[string key] instead.");
        }

        /// <summary>
        /// Mark a list for deletion.
        /// </summary>
        /// <param name="key">Name of the list to mark as deleted</param>
        public new string Remove(string key)
        {
            if (this.ContainsKey(key))
            {
                this[key].IsDeleted = true;
                ManagedList list = (ManagedList)this[key];
                base.Remove(key);
                return list.SavePath;
            }
            return String.Empty;
        }

        public void Save(Stream stream, string name)
        {
            if (this.ContainsKey(name))
            {
                ManagedList list = (ManagedList)this[name];
                list.Save(stream);
            }
        }

        public void SaveAll(StorageFolder folder)
        {
            foreach (var name in this.Keys)
            {
                ManagedList list = (ManagedList)this[name];
                list.Save(folder);
            }
        }

        /// <summary>
        /// Rename a list from a to b.
        /// </summary>
        public void Rename(string a, string b)
        {
            base.Add(b, this[a]);
            this[b].Name = b;
            base.Remove(a);
        }

        /// <summary>
        /// Gets the default, writable list (if there is only one).
        /// </summary>
        /// <returns>The default list or null if there is 0/2+.</returns>
        public DictionaryRecordList DefaultList()
        {
            DictionaryRecordList theList = null;
            foreach (DictionaryRecordList list in this.Values)
                if (!list.ReadOnly && !list.IsDeleted)
                    if (theList != null)
                        return null;
                    else
                        theList = list;
            return theList;
        }

        /// <summary>
        /// Gets the number of writeable (i.e. not read-only) lists.
        /// </summary>
        public int CountWriteable
        {
            get
            {
                int n = 0;
                foreach (DictionaryRecordList list in this.Values)
                    if (!list.ReadOnly && !list.IsDeleted)
                        n++;
                return n;
            }
        }

        const string _ListsDirectory = "lists";
        bool DirectoryExists = false;
        string ListsDirectory
        {
            get
            {
                if (!DirectoryExists)
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        if (!store.DirectoryExists(_ListsDirectory))
                            store.CreateDirectory(_ListsDirectory);
                    DirectoryExists = true;
                }
                return _ListsDirectory;
            }
        }

        ~ListManager()
        {
            foreach (ManagedList list in this.Values)
            {
                if (list.IsDeleted)
                    list.Delete();
                else if (list.IsModified)
                    list.Save();
            }
        }
    }
}
