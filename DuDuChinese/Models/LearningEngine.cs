using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        [Description("Translate to English")]
        Chinese2English,

        [Description("Translate to Chinese")]
        English2Chinese,

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
        // Predefined exercise lists
        private static readonly LearningExercise[] ExerciseListWords = { LearningExercise.Display, LearningExercise.UnderstadningFromHearing };
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

        private static int currentExerciseIndex = 0;
        public static int CurrentExerciseIndex {
            get { return currentExerciseIndex; }
            set
            {
                currentExerciseIndex = value;
                if (currentExerciseIndex >= ExerciseList.Count)
                    currentExerciseIndex = 0;
            }
        }

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

        // Helper function to display enums description
        public static string GetDescription(LearningExercise code)
        {
            Type type = code.GetType();

            System.Reflection.MemberInfo[] memInfo = type.GetMember(code.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = (object[])memInfo[0].GetCustomAttributes(typeof(Description), false);

                if (attrs != null && attrs.Length > 0)
                    return ((Description)attrs[0]).Text + " (" + code.ToString() + ")";
            }

            return code.ToString();
        }
    }
}
