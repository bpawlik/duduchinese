using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        // Display 

        [Description("Display translations")]
        Display,

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

        [Description("Translate from Chinese characters to English")]
        Hanzi2English,

        [Description("Translate from English to Pinyin")]
        English2Pinyin,

        [Description("Translate from English to Chinese characters")]
        English2Hanzi,

        [Description("Translate from Pinyin to Chinese characters")]
        Pinyin2Hanzi,

        [Description("Translate from Chinese characters to Pinyin")]
        Hanzi2Pinyin,

        #endregion

        #region Sentence exercises

        // Textbox

        [Description("Fill gaps")]
        FillGaps,

        #endregion

        [Description("We are at the start fo the big adventure!")]
        Start,

        [Description("All exercises done")]
        Done
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
        // Predefined exercise lists
        private static readonly LearningExercise[] exerciseListWords = {
            LearningExercise.Display,
            LearningExercise.HanziPinyin2English,
            LearningExercise.English2Hanzi,
            LearningExercise.Hanzi2English,
            LearningExercise.Pinyin2Hanzi,
            LearningExercise.English2Pinyin,
            LearningExercise.Hanzi2Pinyin
        };
        private static readonly LearningExercise[] exerciseListSentences = { LearningExercise.Display, LearningExercise.FillGaps };

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

        public static List<LearningExercise> ExerciseList { get; set; } = null;

        private static int currentExerciseIndex = -1;
        public static int CurrentExerciseIndex {
            get
            {
                if (currentExerciseIndex < 0)
                    return 0;
                return currentExerciseIndex;
            }
            private set
            {
                currentExerciseIndex = value;
                if (currentExerciseIndex > ExerciseList.Count)
                    currentExerciseIndex = 0;
            }
        }

        public static LearningExercise PeekNextExercise(out int index)
        {
            index = currentExerciseIndex + 1;
            if (CurrentExercise == LearningExercise.Done || index == ExerciseList.Count)
                return LearningExercise.Done;
            else
                return ExerciseList[index];
        }

        public static LearningExercise NextExercise()
        {
            if (CurrentExercise == LearningExercise.Done)
            {
                // return current exercise (LearningExercise.Done)
            }
            else if (currentExerciseIndex + 1 == ExerciseList.Count)
            {
                currentExerciseIndex = -1;
                CurrentExercise = LearningExercise.Done;
            }
            else
            {
                CurrentExercise = ExerciseList[++currentExerciseIndex];
                if (Mode == LearningMode.Revision)
                    CurrentItemList = RevisionEngine.ToDictionaryRecordList(ListForExercise[CurrentExercise]);
            }
            return CurrentExercise;
        }

        public static void UpdateRevisionList(List<RevisionItem> revisionList)
        {
            ListForExercise.Clear();
            foreach (RevisionItem item in revisionList)
            {
                if (!ListForExercise.ContainsKey(item.Exercise))
                    ListForExercise.Add(item.Exercise, new List<RevisionItem>());
                ListForExercise[item.Exercise].Add(item);
            }

            List<LearningExercise> exerciseList = new List<LearningExercise>();
            foreach (var key in ListForExercise.Keys)
                exerciseList.Add(key);
            ExerciseList = exerciseList;
            currentExerciseIndex = -1;
        }

        public static List<RevisionItem> GenerateLearningItems(DictionaryRecordList recordList)
        {
            List<RevisionItem> items = new List<RevisionItem>();
            foreach (var record in recordList)
            {
                foreach (LearningExercise exercise in ExerciseList)
                    items.Add(new RevisionItem() { Index = record.Index, Exercise = exercise, ListName = recordList.Name, Score = 10 });
            }

            return items;
        }

        public static LearningExercise CurrentExercise { get; private set; } = LearningExercise.Start;

        public static DictionaryRecord CurrentItem { get; private set; } = null;

        private static int currentItemIndex = 0;
        private static DictionaryRecordList currentItemList = null;
        private static List<DictionaryRecord> shuffledItems = null;
        public static DictionaryRecordList CurrentItemList
        {
            get
            {
                return currentItemList;
            }
            set
            {
                currentItemList = value;

                // Shuffle entries
                if (currentItemList != null)
                {
                    var shuffledList = currentItemList.OrderBy(a => Guid.NewGuid());
                    shuffledItems = new List<DictionaryRecord>(shuffledList);
                }
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

                // Shuffle entries
                if (currentItemList != null)
                {
                    var shuffledList = currentItemList.OrderBy(a => Guid.NewGuid());
                    shuffledItems = new List<DictionaryRecord>(shuffledList).GetRange(0, itemsCount);
                }
            }
        }

        private static Dictionary<LearningExercise, List<RevisionItem> > listForExercise = null;
        public static Dictionary<LearningExercise, List<RevisionItem> > ListForExercise
        {
            get
            {
                if (listForExercise == null)
                    listForExercise = new Dictionary<LearningExercise, List<RevisionItem>>();
                return listForExercise;
            }
            set
            {
                listForExercise = value;
            }
        }

        public static int GetItemCountForExercise(LearningExercise exercise)
        {
            if (Mode == LearningMode.Revision)
                return ListForExercise[exercise].Count;
            else
                return CurrentItemList.Count;
        }

        public static void SetVisibility(out Visibility PinyinVisible, out Visibility TranslationVisible, out Visibility SimplifiedVisible)
        {
            switch (ExerciseList[CurrentExerciseIndex])
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
            currentItemList = null;
            correctCount = 0;
            wrongCount = 0;
        }

        public static DictionaryRecord GetNextItem()
        {
            if (shuffledItems == null || currentItemIndex >= shuffledItems.Count)
            {
                currentItemIndex = 0;
                return null;
            }
            else
            {
                CurrentItem = shuffledItems[currentItemIndex++];
                return CurrentItem;
            }
        }

        private static int correctCount = 0;
        private static int wrongCount = 0;

        public static string GetStatus()
        {
            if (correctCount > 0 || wrongCount > 0)
            {
                int totalItems = shuffledItems.Count;
                string score = "Total: " + ((int)(100.0 * Convert.ToDouble(correctCount) / Convert.ToDouble(totalItems))).ToString() + " %" + Environment.NewLine;
                string correct = "Correct: " + correctCount.ToString() + Environment.NewLine;
                string wrong = "Wrong: " + wrongCount.ToString() + Environment.NewLine;
                
                return correct + wrong + score;
            }
            else
            {
                return "";
            }
        }

        public static bool Validate(string inputText)
        {
            if (currentItemIndex > shuffledItems.Count)
                return false;

            bool result = false;
            inputText = inputText.ToLower();

            switch (ExerciseList[CurrentExerciseIndex])
            {
                case LearningExercise.Display:
                    return true;
                case LearningExercise.HanziPinyin2English:
                case LearningExercise.Hanzi2English:
                case LearningExercise.Pinyin2English:
                    foreach (string s in CurrentItem.English)
                    {
                        if (String.IsNullOrWhiteSpace(s) || String.IsNullOrWhiteSpace(inputText))
                            continue;

                        string refText = s.ToLower();

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
            if (Mode == LearningMode.Revision)
                RevisionEngine.UpdateRevisionList(ListForExercise[CurrentExercise][currentItemIndex - 1], result);
            else
                RevisionEngine.UpdateRevisionList(CurrentItemList.Name, CurrentItem, ExerciseList[CurrentExerciseIndex], result);

            return result;
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
