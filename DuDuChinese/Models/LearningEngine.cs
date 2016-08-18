using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;

namespace DuDuChinese.Models
{
    public enum LearningMode
    {
        Words = 0,
        Sentences = 1,
        Revision = 2
    }

    public enum LearningExercise
    {
        #region Word exercises

        // Radio 

        [Description("Choose pinyin from hearing")]
        ChoosePinyinFromHearing,

        [Description("Choose character from hearing")]
        ChooseHanziFromHearing,

        [Description("Choose translation from hearing")]
        ChooseEnglishFromHearing,

        // Textbox

        [Description("Dictation")]
        Dictation,

        [Description("Write translation from hearing")]
        Translation,

        // Yes / No

        [Description("Translate from Chinese to English")]
        HanziPinyin2English,

        [Description("Translate from Pinyin to English")]
        Pinyin2English,

        [Description("Translate from 汉字 to English")]
        Hanzi2English,

        [Description("Translate from English to Pinyin")]
        English2Pinyin,

        [Description("Translate from English to 汉字")]
        English2Hanzi,

        [Description("Translate from Pinyin to 汉字")]
        Pinyin2Hanzi,

        [Description("Translate from 汉字 to Pinyin")]
        Hanzi2Pinyin,

        #endregion

        #region Sentence exercises

        // Textbox

        [Description("Fill gaps with English")]
        FillGapsEnglish,

        [Description("Fill gaps with 汉字")]
        FillGapsChinese,

        #endregion

        #region Common exercises

        [Description("Display learning items")]
        Display,

        [Description("We are at the start fo the big adventure!")]
        Start,

        [Description("All exercises done")]
        Done

        #endregion
    }

    public class Description : Attribute
    {
        public string Text { set; get; }

        public Description(string text)
        {
            this.Text = text;
        }
    }

    public class LearningEngine
    {
        #region Predefined exercise lists

        private static readonly LearningExercise[] exerciseListWords = {
            LearningExercise.Display,
            LearningExercise.HanziPinyin2English,
            LearningExercise.English2Hanzi,
            LearningExercise.Hanzi2English,
            LearningExercise.Pinyin2Hanzi,
            LearningExercise.English2Pinyin,
            LearningExercise.Hanzi2Pinyin
        };

        private static readonly LearningExercise[] exerciseListSentences = {
            LearningExercise.Display,
            LearningExercise.FillGapsChinese
        };

        #endregion

        #region Properties

        private static List<LearningExercise> ExerciseList { get; set; } = null;
        public static List<LearningExercise> CurrentExerciseList { get; private set; } = null;
        public static LearningExercise CurrentExercise { get; private set; } = LearningExercise.Start;
        public static DictionaryRecord CurrentItem { get; private set; } = null;
        private static int currentItemIndex = 0;
        private static int correctCount = 0;
        private static int wrongCount = 0;

        private static LearningMode mode = LearningMode.Words;
        public static LearningMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                if (mode == LearningMode.Words)
                    ExerciseList = new List<LearningExercise>(exerciseListWords);
                else if (mode == LearningMode.Sentences)
                    ExerciseList = new List<LearningExercise>(exerciseListSentences);
            }
        }

        private static int itemsCount = 0;
        public static int ItemsCount
        {
            get
            {
                return itemsCount;
            }
            set
            {
                itemsCount = value;

                if (learningItems == null)
                    return;

                // Shuffle entries
                if (Mode == LearningMode.Revision)
                {
                    // Shuffle keys
                    var keys = learningItems.Keys;
                    var shuffledKeys = keys.OrderBy(a => Guid.NewGuid());

                    // Shuffle lists
                    int counter = 0;
                    foreach (var key in shuffledKeys)
                    {
                        var shuffledItems = new List<LearningItem>(learningItems[key].OrderBy(a => Guid.NewGuid()));
                        learningItems[key].Clear();
                        for (int i = 0; i < shuffledItems.Count && counter < itemsCount; ++i, ++counter)
                            learningItems[key].Add(shuffledItems[i]);
                    }
                }
                else
                {
                    // Shuffle lists
                    var keys = new List<LearningExercise>(learningItems.Keys);
                    foreach (var key in keys)
                    {
                        var shuffledItems = learningItems[key].OrderBy(a => Guid.NewGuid());
                        int count = Math.Min(learningItems[key].Count, itemsCount);
                        learningItems[key] = new List<LearningItem>(shuffledItems).GetRange(0, count);
                    }
                }
            }
        }

        private static Dictionary<LearningExercise, List<LearningItem>> learningItems = null;
        public static Dictionary<LearningExercise, List<LearningItem>> LearningItems
        {
            get
            {
                if (learningItems == null)
                    learningItems = new Dictionary<LearningExercise, List<LearningItem>>();
                return learningItems;
            }
            set
            {
                learningItems = value;
            }
        }

        private static int currentExerciseIndex = -1;
        private static int CurrentExerciseIndex {
            get
            {
                if (currentExerciseIndex < 0)
                    return 0;
                return currentExerciseIndex;
            }
            set
            {
                currentExerciseIndex = value;
                if (currentExerciseIndex > CurrentExerciseList.Count)
                    currentExerciseIndex = 0;
            }
        }

        public static bool IsSentence
        {
            get
            {
                if (Mode == LearningMode.Sentences)
                    return true;

                switch (CurrentExercise)
                {
                    case LearningExercise.FillGapsChinese:
                    case LearningExercise.FillGapsEnglish:
                        return true;
                    default:
                        return false;
                }
            }
        }

        #endregion

        public static LearningExercise PeekNextExercise(out int index)
        {
            index = currentExerciseIndex + 1;
            if (CurrentExercise == LearningExercise.Done || index == CurrentExerciseList.Count)
                return LearningExercise.Done;
            else
                return CurrentExerciseList[index];
        }

        public static LearningExercise NextExercise()
        {
            if (CurrentExercise == LearningExercise.Done)
            {
                // return current exercise (LearningExercise.Done)
            }
            else if (currentExerciseIndex + 1 == CurrentExerciseList.Count)
            {
                currentExerciseIndex = -1;
                CurrentExercise = LearningExercise.Done;
            }
            else
            {
                CurrentExercise = CurrentExerciseList[++currentExerciseIndex];
            }

            // Reset counters
            correctCount = 0;
            wrongCount = 0;

            return CurrentExercise;
        }

        public static void UpdateLearningList(List<LearningItem> learningList)
        {
            LearningItems.Clear();
            foreach (LearningItem item in learningList)
            {
                if (!LearningItems.ContainsKey(item.Exercise))
                    LearningItems.Add(item.Exercise, new List<LearningItem>());
                LearningItems[item.Exercise].Add(item);
            }

            List<LearningExercise> exerciseList = new List<LearningExercise>();
            foreach (var key in LearningItems.Keys)
                exerciseList.Add(key);
            CurrentExerciseList = exerciseList;
            currentExerciseIndex = -1;
        }

        public static int GenerateLearningItems(DictionaryRecordList recordList)
        {
            List<LearningItem> allItems = new List<LearningItem>();
            foreach (var record in recordList)
            {
                foreach (LearningExercise exercise in ExerciseList)
                    allItems.Add(new LearningItem(record) { Exercise = exercise, ListName = recordList.Name, Score = 10 });
            }

            // Remove overlapping items between new material and revisions
            List<LearningItem> revisionItems = RevisionEngine.RevisionList;
            if (allItems.Count > 0 && revisionItems != null)
            {
                // Get the intersection of learningItems and revisionItems
                var intersectList = new List<LearningItem>(allItems.Intersect(revisionItems));

                // Remove those items from the list that overlap
                foreach (var item in intersectList)
                    allItems.Remove(item);
            }

            // If learning sentences then select only those word that have sentence defined
            List<LearningItem> selectedItems;
            if (Mode == LearningMode.Sentences)
                selectedItems = new List<LearningItem>(allItems.Where(i => i.Record.Sentence.Count == 2));
            else
                selectedItems = allItems;

            // Update learning list and return items count
            UpdateLearningList(selectedItems);

            if (Mode == LearningMode.Revision)
                return selectedItems.Count;
            else if (learningItems.Count > 0)
                return LearningItems.First().Value.Count;
            return 0;
        }

        public static int GetItemCountForCurrentExercise()
        {
            return GetItemCountForExercise(CurrentExercise);
        }

        public static int GetItemCountForExercise(LearningExercise exercise)
        {
            return (LearningItems == null || !LearningItems.ContainsKey(exercise)) ? 0
                    : LearningItems[exercise].Count;
        }

        public static void SetVisibility(out Visibility PinyinVisible, out Visibility TranslationVisible, out Visibility SimplifiedVisible, out Visibility SentenceVisible)
        {
            SentenceVisible = CurrentExercise == LearningExercise.Display ? Visibility.Visible : Visibility.Collapsed;
            switch (CurrentExerciseList[CurrentExerciseIndex])
            {
                case LearningExercise.Display:
                    PinyinVisible = Visibility.Visible;
                    TranslationVisible = Visibility.Visible;
                    SimplifiedVisible = Visibility.Visible;
                    break;
                case LearningExercise.HanziPinyin2English:
                    PinyinVisible = Visibility.Visible;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Visible;
                    break;
                case LearningExercise.English2Hanzi:
                case LearningExercise.English2Pinyin:
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Visible;
                    SimplifiedVisible = Visibility.Collapsed;
                    break;
                case LearningExercise.Pinyin2Hanzi:
                case LearningExercise.Pinyin2English:
                    PinyinVisible = Visibility.Visible;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Collapsed;
                    break;
                case LearningExercise.Hanzi2English:
                case LearningExercise.Hanzi2Pinyin:
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Visible;
                    break;
                case LearningExercise.FillGapsChinese:
                case LearningExercise.FillGapsEnglish:
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Collapsed;
                    break;
                default:
                    PinyinVisible = Visibility.Visible;
                    TranslationVisible = Visibility.Visible;
                    SimplifiedVisible = Visibility.Visible;
                    break;
            }
        }

        public static void Reset()
        {
            currentExerciseIndex = -1;
            CurrentExercise = LearningExercise.Start;
            currentItemIndex = 0;
            LearningItems = null;
            correctCount = 0;
            wrongCount = 0;
        }

        public static DictionaryRecord GetNextItem()
        {
            if (LearningItems == null || currentItemIndex >= LearningItems[CurrentExercise].Count)
            {
                currentItemIndex = 0;
                return null;
            }
            else
            {
                LearningItem item = LearningItems[CurrentExercise][currentItemIndex++];
                if (item.Record == null)
                {
                    App app = (App)Application.Current;
                    item.Record = app.ListManager[item.ListName].Find(r => LearningItem.ComputeHash(r) == item.Hash);
                }
                CurrentItem = item.Record;
                return CurrentItem;
            }
        }

        public static string GetStatus()
        {
            if (correctCount > 0 || wrongCount > 0)
            {
                int totalItems = LearningItems[CurrentExercise].Count;
                string score = "Total: " + ((int)(100.0 * Convert.ToDouble(correctCount) / Convert.ToDouble(totalItems))).ToString() + " %" + Environment.NewLine;
                string correct = "Correct: " + correctCount.ToString() + Environment.NewLine;
                string wrong = "Wrong: " + wrongCount.ToString() + Environment.NewLine;
                
                return correct + wrong + score;
            }

            return String.Empty;
        }

        public static bool Validate(string inputText)
        {
            if (currentItemIndex > LearningItems[CurrentExercise].Count)
                return false;

            bool result = false;
            inputText = inputText.ToLower();

            switch (CurrentExerciseList[CurrentExerciseIndex])
            {
                case LearningExercise.Display:
                    return true;
                case LearningExercise.HanziPinyin2English:
                case LearningExercise.Hanzi2English:
                case LearningExercise.Pinyin2English:
                case LearningExercise.FillGapsEnglish:
                    foreach (string s in CurrentItem.English)
                    {
                        if (String.IsNullOrWhiteSpace(s) || String.IsNullOrWhiteSpace(inputText))
                            continue;

                        // Convert to lower-case and replace comas/dots/colons/brackets with spaces
                        string refText = s.ToLower().Replace(',',' ').Replace('.',' ').Replace(';',' ').Replace(':',' ').Replace('(',' ').Replace(')',' ');

                        // If input text contains space then match it as a whole phrase
                        if (inputText.Contains(" "))
                        {
                            if (refText == inputText)
                            {
                                result = true;
                                break;
                            }
                        }
                        else  // match input string with any of the words from the translation
                        {
                            List<string> temp = new List<string>(refText.Split(' '));
                            foreach (string sInner in temp)
                            {
                                if (sInner == inputText)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case LearningExercise.English2Hanzi:
                case LearningExercise.Pinyin2Hanzi:
                case LearningExercise.FillGapsChinese:
                    result = CurrentItem.Chinese.Simplified == inputText;
                    break;
                case LearningExercise.Hanzi2Pinyin:
                case LearningExercise.English2Pinyin:
                    result = CurrentItem.Chinese.PinyinNoMarkup == inputText;
                    break;
            }

            if (result)
                correctCount++;
            else
                wrongCount++;

            // Update revision list
            RevisionEngine.UpdateRevisionList(LearningItems[CurrentExercise][currentItemIndex - 1], result);

            // Put item at the end of the list if wrong
            if (!result)
                LearningItems[CurrentExercise].Add(LearningItems[CurrentExercise][currentItemIndex - 1]);

            return result;
        }

        public static void RevertLastValidate()
        {
            wrongCount--;
            LearningItems[CurrentExercise].RemoveAt(LearningItems[CurrentExercise].Count - 1);
        }

        // Helper function to display enums description
        public static string GetDescription(LearningExercise code)
        {
            Type type = code.GetType();

            System.Reflection.MemberInfo[] memInfo = type.GetMember(code.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = (object[])memInfo[0].GetCustomAttributes(typeof(Description), false);

                if (attrs != null && attrs.Length > 0)
                    return ((Description)attrs[0]).Text;
            }

            return code.ToString();
        }
    }
}
