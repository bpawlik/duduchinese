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
        Sentences = 1
    }

    public enum LearningExercise
    {
        // Display 

        [Description("Display translations")]
        Display,

        #region Word exercises

        // Radio 

        [Description("Understanding from hearing")]
        UnderstadningFromHearing,

        [Description("Text understanding")]
        TextUnderstanding,

        [Description("Select translation")]
        TranslationSelection,

        // Textbox

        [Description("Dictation")]
        Dictation,

        [Description("Write translation from hearing")]
        Translation,

        #endregion

        #region Sentence exercises

        // Textbox

        [Description("Fill gaps")]
        FillGaps,

        // Yes / No

        [Description("Translate from Hanzi + Pinyin to English")]
        HanziPinyin2English,

        [Description("Translate from Pinyin to English")]
        Pinyin2English,

        [Description("Translate from Hanzi to English")]
        Hanzi2English,

        [Description("Translate from English to Pinyin")]
        English2Pinyin,

        [Description("Translate from English to Hanzi")]
        English2Hanzi,

        [Description("Translate from Pinyin to Hanzi")]
        Pinyin2Hanzi,

        [Description("Translate from Hanzi to Pinyin")]
        Hanzi2Pinyin,

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
        private static readonly LearningExercise[] ExerciseListWords = {
            LearningExercise.Display,
            LearningExercise.HanziPinyin2English,
            LearningExercise.English2Hanzi
        };
        private static readonly LearningExercise[] ExerciseListSentences = { LearningExercise.Display, LearningExercise.FillGaps };

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
                    ExerciseList = new List<LearningExercise>(ExerciseListWords);
                else
                    ExerciseList = new List<LearningExercise>(ExerciseListSentences);
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

        public static LearningExercise NextExercise()
        {
            if (CurrentExercise == LearningExercise.Done)
            {
                // return current exercise
            }
            else if (currentExerciseIndex + 1 == ExerciseList.Count)
            {
                currentExerciseIndex = -1;
                CurrentExercise = LearningExercise.Done;
            }
            else
            {
                CurrentExercise = ExerciseList[++currentExerciseIndex];
            }
            return CurrentExercise;
        }

        public static LearningExercise CurrentExercise { get; set; } = LearningExercise.Start;

        private static int currentItemIndex = 0;
        private static DictionaryItemList currentItemList = null;
        private static List<DictionaryItem> shuffledItems = null;
        public static DictionaryItemList CurrentItemList
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
                    var shuffledList = currentItemList.Words.OrderBy(a => Guid.NewGuid());
                    shuffledItems = new List<DictionaryItem>(shuffledList);
                }
            }
        }

        public static void SetVisibility(ref Visibility PinyinVisible, ref Visibility TranslationVisible, ref Visibility SimplifiedVisible)
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
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Visible;
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
            currentItemList = null;
        }

        public static bool EndOfItemList()
        {
            return currentItemIndex >= shuffledItems.Count;
        }

        public static DictionaryItem GetNextItem()
        {
            if (shuffledItems == null || currentItemIndex >= shuffledItems.Count)
            {
                currentItemIndex = 0;
                return null;
            }
            else
            {
                return shuffledItems[currentItemIndex++];
            }
        }

        public static string GetStatus()
        {
            string temp = currentItemIndex.ToString() + " / " + shuffledItems.Count.ToString();
            return temp;
        }

        public static bool Validate(string inputText)
        {
            if (currentItemIndex >= shuffledItems.Count)
                return false;

            DictionaryItem currentItem = shuffledItems[currentItemIndex];
            switch (ExerciseList[CurrentExerciseIndex])
            {
                case LearningExercise.Display:
                    return true;
                case LearningExercise.HanziPinyin2English:
                case LearningExercise.Hanzi2English:
                case LearningExercise.Pinyin2English:
                    foreach (string s in currentItem.Translation)
                    {
                        if (String.IsNullOrWhiteSpace(s) || String.IsNullOrWhiteSpace(inputText))
                            continue;
                        if (s.Contains(inputText))
                            return true;
                    }
                    return false;
                case LearningExercise.English2Hanzi:
                    return currentItem.Simplified == inputText;
                case LearningExercise.Pinyin2Hanzi:
                    return string.Join(" ", currentItem.Pinyin.ToArray()) == inputText;
                default:
                    return false;
            }
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
