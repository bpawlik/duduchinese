﻿using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DuDuChinese.Models
{
    public class RevisionEngine
    {
        private static readonly string revisionsFile = "revisions.xml";

        // DictionaryItem - learning exercise - score (10, +1 for wrong, -1 for correct)
        // Prioritize items with highest score
        private static List<LearningItem> revisionList = null;

        [DataMember]
        public static List<LearningItem> RevisionList
        {
            get
            {
                if (revisionList == null)
                {
                    Deserialize();
                }
                return revisionList;
            }

            private set
            {
                if (value != null)
                    revisionList = value;
            }
        }

        public static List<LearningItem> GetRevisionList(int numberOfItems, string name = null)
        {
            if (numberOfItems == 0 || revisionList == null)
                return new List<LearningItem>();

            if (numberOfItems < 0 || numberOfItems > revisionList.Count)
                numberOfItems = revisionList.Count;

            // Sort item by the score
            revisionList.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));

            App app = (App)Application.Current;

            // Remove revision items which...
            int removedCount = revisionList.RemoveAll(item => (
                !app.ListManager.ContainsKey(item.ListName) ||  // ...list they belong to doesn't exist anymore
                app.ListManager[item.ListName].Count == 0 ||    // ...list they belong to is empty...
                app.ListManager[item.ListName].Find(r => LearningItem.ComputeHash(r) == item.Hash) == null  //  ...or doesn't exist in the list
            ));
            if (removedCount > 0)
                Serialize();

            // Fill list with the worst items
            List<LearningItem> revList = new List<LearningItem>();
            for (int i = 0; i < numberOfItems && i < revisionList.Count; ++i)
            {
                if (name != null && revisionList[i].ListName != name)
                    continue;

                // Skip items having satisfying score (0)
                if (revisionList[i].Score == 0)
                {
                    numberOfItems++;
                    continue;
                }

                revList.Add(revisionList[i]);
            }

            return revList;
        }

        public static void UpdateRevisionList(LearningItem revItem, bool correct)
        {
            // Find given record and exercise
            var find = RevisionList.FirstOrDefault(x => (revItem == x));
            if (find != null)
            {
                find.Score += (correct ? -1 : 1);
            }
            else
            {
                RevisionList.Add(revItem);
            }
        }

        public static void UpdateRevisionList(string listName, DictionaryRecord record, LearningExercise exercise, bool correct)
        {
            // Find given record and exercise
            var find = RevisionList.FirstOrDefault(x => (x.ListName == listName && x.Hash == LearningItem.ComputeHash(record) && x.Exercise == exercise));
            if (find != null)
            {
                find.Score += (correct ? -1 : 1);
            }
            else
            {
                RevisionList.Add(new LearningItem(record) { ListName = listName, Exercise = exercise, Score = (10 + (correct ? -1 : 1)) });
            }
        }

        public static async void Serialize()
        {
            // Get the local folder.
            StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new file named data_file.xml
            StorageFile file = await data_folder.CreateFileAsync(revisionsFile, CreationCollisionOption.ReplaceExisting);

            // Write the data
            using (Stream s = await file.OpenStreamForWriteAsync())
            {
                // Write an object to the Stream and leave it opened
                using (var writer = XmlDictionaryWriter.CreateTextWriter(s, Encoding.UTF8, ownsStream: false))
                {
                    var ser = new DataContractSerializer(typeof(List<LearningItem>));
                    ser.WriteObject(writer, revisionList);
                }
            }
            await Task.CompletedTask;
        }

        public static async void Deserialize()
        {
            try
            {
                // Get the local folder.
                StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

                if (!File.Exists(Path.Combine(data_folder.Path, revisionsFile)))
                {
                    revisionList = new List<LearningItem>();
                    return;
                }

                // Create a new file named data_file.xml
                StorageFile file = await data_folder.GetFileAsync(revisionsFile);

                // Write the data
                using (Stream s = await file.OpenStreamForReadAsync())
                {
                    // Read Stream to the Serializer and Deserialize and close it
                    using (var reader = XmlDictionaryReader.CreateTextReader(s, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null))
                    {
                        var serializer = new DataContractSerializer(typeof(List<LearningItem>));
                        revisionList = (List<LearningItem>)serializer.ReadObject(reader);
                    }
                }
            }
            catch (XmlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't deserialize revision list. Probably it is empty. Error: " + ex.Message);
                revisionList = new List<LearningItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't deserialize revision list! " + ex.Message);
                revisionList = null;
            }

            await Task.CompletedTask;
        }
    }
}
