﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading;
using Windows.Storage;

namespace CC_CEDICT.Universal
{
    public class Index : StreamLineArray<IndexRecord>
    {
        bool loaded = false;
        Dictionary<string, int> lookup = new Dictionary<string, int>();

        public Index(string name)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(name))
                {
                    Debug.WriteLine(String.Format("Loading index file: {0}", name));
                    Stream stream = new IsolatedStorageFileStream(name, FileMode.Open, store);
                    if (!stream.CanRead)
                        return;
                    loaded = Read(stream);
                }
            }
        }

        public IndexRecord this[string key]
        {
            get
            {
                if (!loaded)
                    return null;
                key = key.ToLower();
                if (!lookup.ContainsKey(key))
                    lookup[key] = BinarySearch(key);
                return lookup[key] < 0 ? null : this[lookup[key]];
            }
        }

        int BinarySearch(string key)
        {
            CompareInfo ci = new CultureInfo("en-US").CompareInfo;
            int min = 0, pos, max = this.Count - 1;
            while (min <= max)
            {
                pos = (min + max) / 2;
                switch (ci.Compare(key, this[pos].Key, CompareOptions.StringSort))
                {
                    case -1:
                        max = pos - 1;
                        Debug.WriteLine("BinarySearch: compare {0} to {1} (down)", key, this[pos].Key);
                        break;
                    case 0:
                        Debug.WriteLine("BinarySearch: compare {0} to {1} (MATCH)", key, this[pos].Key);
                        return pos;
                    case 1:
                        min = pos + 1;
                        Debug.WriteLine("BinarySearch: compare {0} to {1} (up)", key, this[pos].Key);
                        break;
                }
            }
            Debug.WriteLine("BinarySearch: not found.");
            return -1;
        }
    }
}
