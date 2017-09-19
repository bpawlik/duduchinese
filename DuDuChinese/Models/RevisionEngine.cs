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

        public static List<LearningItem> GetRevisionList(int numberOfItems = -1, string name = null, bool fullList = false)
        {
            if (numberOfItems == 0 || revisionList == null)
                return new List<LearningItem>();

            if (numberOfItems < 0 || numberOfItems > revisionList.Count)
                numberOfItems = revisionList.Count;

            // Sort item by the score in descending order
            revisionList.Sort((s1, s2) => s2.Score.CompareTo(s1.Score));

            App app = (App)Application.Current;

            // Remove revision items which...
            int removedCount = revisionList.RemoveAll(item => (
                !app.ListManager.ContainsKey(item.ListName) ||  // ...list they belong to doesn't exist anymore
                app.ListManager[item.ListName].Count == 0 ||    // ...list they belong to is empty...
                app.ListManager[item.ListName].Find(r => LearningItem.ComputeHash(r) == item.Hash) == null  //  ...or doesn't exist in the list
            ));
            if (removedCount > 0)
                Serialize();

            // Black list
            List<LearningExercise> blackList = new List<LearningExercise>() {
                LearningExercise.English2Pinyin,
                LearningExercise.Pinyin2English,
                LearningExercise.Pinyin2Hanzi,
                LearningExercise.FillGapsEnglish
            };

            // Fill list with the worst items (but not already learnt today)
            List<LearningItem> revList = new List<LearningItem>();
            DateTime today = DateTime.Today;
            for (int i = 0; i < numberOfItems && i < revisionList.Count; ++i)
            {
                if (name != null && revisionList[i].ListName != name)
                    continue;

                // Skip items having satisfying score (0) and those that are not scheduled for today or those listed on the black list
                if (!fullList && (revisionList[i].Score == 0 || revisionList[i].Timestamp.Date > today || blackList.Contains(revisionList[i].Exercise)))
                {
                    numberOfItems++;
                    continue;
                }

                revList.Add(revisionList[i]);
            }

            return revList;
        }

        public static void UpdateRevisionList(LearningItem revItem, bool correct, int addend = 1)
        {
            // Find given record and exercise
            var find = RevisionList.FirstOrDefault(x => (revItem == x));
            if (find == null)
            {
                RevisionList.Add(revItem);
                find = revItem;
            }

            // Increase or decrease score based on result
            find.Score += (correct ? -addend : addend);
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

        public static void RenameList(string oldName, string newName)
        {
            foreach (var item in RevisionList)
            {
                if (item.ListName == oldName)
                    item.ListName = newName;
            }

            Serialize();
        }

        public static async void Serialize()
        {
            // Get the local folder.
            StorageFolder data_folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // If revision list is null or empty, but file exists, then try to deserialize
            if ((revisionList == null || (revisionList != null && revisionList.Count == 0))
                && File.Exists(Path.Combine(data_folder.Path, revisionsFile)))
            {
                await Deserialize();

                // Never ever serialize empty list!
                await Task.CompletedTask;
                return;
            }

            // Create a new file named data_file.xml
            StorageFile file = await data_folder.CreateFileAsync(revisionsFile, CreationCollisionOption.ReplaceExisting);

            // Write the data
            using (Stream s = await file.OpenStreamForWriteAsync())
            {
                Serialize(s);
            }
            await Task.CompletedTask;
        }

        public static void Serialize(Stream stream)
        {
// Move past items to the future if you've been lazy ;)
#if LAZY_MODE
            List<LearningItem> revList = new List<LearningItem>();
            DateTime today = DateTime.Today;
            Random rnd = new Random();
            for (int i = 0; i < revisionList.Count; ++i)
            {
                // Move old items to the future
                if (revisionList[i].Score != 0 && revisionList[i].Timestamp.Date < today)
                    revisionList[i].Timestamp = today.AddDays(rnd.Next(1, 30));
            }
#endif

            // Write an object to the Stream and leave it opened
            using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, ownsStream: false))
            {
                var ser = new DataContractSerializer(typeof(List<LearningItem>));
                ser.WriteObject(writer, revisionList);
            }
        }

        public static void SaveToCsv(Stream stream)
        {
            // Write an object to the Stream and leave it opened
            StringBuilder sb = new StringBuilder();
            string csvDelimiter = ",\t";

            // Write the header first
            sb.AppendLine(
                "Character(s)" + csvDelimiter
                + "List" + csvDelimiter
                + "Score" + csvDelimiter
                + "Next review" + csvDelimiter
                + "Exercise");

            // Dump revision items line by line
            foreach (LearningItem item in revisionList)
                sb.AppendLine(
                    item.Record.Chinese.Simplified + csvDelimiter
                    + item.ListName + csvDelimiter
                    + item.Score + csvDelimiter
                    + item.Timestamp.ToString("dd-MM-yyyy") + csvDelimiter
                    + item.Exercise);

            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            stream.Write(data, 0, data.Length);
        }

        public static async Task Deserialize()
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
                    Deserialize(s);
                }

                if (revisionList == null)
                    revisionList = new List<LearningItem>();

                // Remove duplicates
                System.Diagnostics.Debug.WriteLine("Revision list size: " + revisionList.Count);
                revisionList = revisionList.Distinct(new LearningItemComparer()).ToList();
                System.Diagnostics.Debug.WriteLine("Revision list size (after removing duplicates): " + revisionList.Count);

                // Sort the list in descending order
                revisionList.Sort((s1, s2) => s2.Score.CompareTo(s1.Score));
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

        public static void Deserialize(Stream stream)
        {
            // Read Stream to the Serializer and Deserialize and close it
            using (var reader = XmlDictionaryReader.CreateTextReader(stream, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null))
            {
                var serializer = new DataContractSerializer(typeof(List<LearningItem>));
                revisionList = (List<LearningItem>)serializer.ReadObject(reader);
            }
        }
    }
}
