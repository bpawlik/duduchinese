using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using DuDuChinese.Models;
using Windows.UI.Xaml;

namespace DuDuChinese.Models
{
    [DataContract]
    public class RevisionItem
    {
        [DataMember]
        public int Index;

        [DataMember]
        public string ListName;

        [DataMember]
        public LearningExercise Exercise;

        [DataMember]
        public int Score;

        public RevisionItem()
        {
            Score = 10;
        }
    }

    public class RevisionEngine
    {
        private static readonly string revisionsFile = "revisions.xml";

        // DictionaryItem - learning exercise - score (10, +1 for wrong, -1 for correct)
        // Priritize items with highest score
        private static List<RevisionItem> revisionList = null;

        [DataMember]
        public static List<RevisionItem> RevisionList
        {
            get {
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

        public static List<RevisionItem> GetRevisionList(int numberOfItems, string name = null)
        {
            if (numberOfItems == 0 || revisionList == null)
                return new List<RevisionItem>();

            if (numberOfItems < 0 || numberOfItems > revisionList.Count)
                numberOfItems = revisionList.Count;

            // Sort item by the score
            revisionList.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));

            App app = (App)Application.Current;

            // Fill list with the worst items
            List<RevisionItem> revList = new List<RevisionItem>();
            for (int i = 0; i < numberOfItems && i < revisionList.Count; ++i)
            {
                if (name != null && revisionList[i].ListName != name)
                    continue;

                revList.Add(revisionList[i]);
            }

            return revList;
        }

        public static void UpdateRevisionList(RevisionItem revItem, bool correct)
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
            var find = RevisionList.FirstOrDefault(x => (x.ListName == listName && x.Index == record.Index && x.Exercise == exercise));
            if (find != null)
            {
                find.Score += (correct ? -1 : 1);
            }
            else
            {
                RevisionList.Add(new RevisionItem() { ListName = listName, Index = record.Index, Exercise = exercise, Score = (10 + (correct ? -1 : 1)) });
            }
        }

        // Conversion form List<RevisionItem> to DicitonaryRecordList
        public static DictionaryRecordList ToDictionaryRecordList(List<RevisionItem> revList)
        {
            App app = (App)Application.Current;
            DictionaryRecordList recordList = new DictionaryRecordList("revision");
            foreach (RevisionItem item in revList)
            {
                DictionaryRecord record = app.ListManager[item.ListName][item.Index];
                recordList.Add(record);
            }

            return recordList;
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
                //var ms = new MemoryStream();
                // Write an object to the Stream and leave it opened
                using (var writer = XmlDictionaryWriter.CreateTextWriter(s, Encoding.UTF8, ownsStream: false))
                {
                    var ser = new DataContractSerializer(typeof(List<RevisionItem>));
                    ser.WriteObject(writer, revisionList);
                }
            }
        }

        public static async void Deserialize()
        {
            try
            {
                // Get the local folder.
                StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

                if (!File.Exists(Path.Combine(data_folder.Path, revisionsFile)))
                {
                    revisionList = new List<RevisionItem>();
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
                        var serializer = new DataContractSerializer(typeof(List<RevisionItem>));
                        revisionList = (List<RevisionItem>)serializer.ReadObject(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

    }
}
