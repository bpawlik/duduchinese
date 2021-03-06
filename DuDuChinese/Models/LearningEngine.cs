﻿using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        [Description("Translate from Chinese to English")]
        [Command("Input English")]
        HanziPinyin2English,

        [Description("Translate from Pinyin to English")]
        [Command("Input English")]
        Pinyin2English,

        [Description("Translate from 汉字 to English")]
        [Command("Input English")]
        Hanzi2English,

        [Description("Translate from English to pin1yin1")]
        [Command("Input pin1yin1")]
        English2Pinyin,

        [Description("Translate from English to 汉字")]
        [Command("Input 汉字")]
        English2Hanzi,

        [Description("Translate from pin1yin1 to 汉字")]
        [Command("Input 汉字")]
        Pinyin2Hanzi,

        [Description("Translate from 汉字 to pin1yin1")]
        [Command("Input pin1yin1")]
        Hanzi2Pinyin,

        [Description("Write 汉字")]
        [Command("Draw 汉字")]
        DrawHanzi,

        #endregion

        #region Sentence exercises

        // Textbox

        [Description("Fill gaps with English")]
        [Command("Input English")]
        FillGapsEnglish,

        [Description("Fill gaps with 汉字")]
        [Command("Input 汉字")]
        FillGapsChinese,

        [Description("Translate sentence to 汉字")]
        [Command("Input 汉字")]
        EnglishSentence2Hanzi,

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

    public interface IText
    {
        string GetText();
    }

    public class Description : Attribute, IText
    {
        private string text;

        public Description(string text)
        {
            this.text = text;
        }

        public string GetText()
        {
            return this.text;
        }
    }

    public class Command : Attribute, IText
    {
        private string text;

        public Command(string text)
        {
            this.text = text;
        }

        public string GetText()
        {
            return this.text;
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
            LearningExercise.Hanzi2Pinyin,
            LearningExercise.DrawHanzi
        };

        private static readonly LearningExercise[] exerciseListSentences = {
            LearningExercise.Display,
            LearningExercise.FillGapsChinese,
            LearningExercise.EnglishSentence2Hanzi
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
        private static bool lastResult = true;

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
                    // If we've got only display exercises then we have nothing to learn anymore from this list
                    if (learningItems.Count == 0 || (learningItems.Keys.Count == 1 && learningItems.First().Key == LearningExercise.Display))
                    {
                        itemsCount = 0;
                        return;
                    }

                    // Consider Display exercise only
                    var shuffledDisplayItems = learningItems[LearningExercise.Display].OrderBy(a => Guid.NewGuid());

                    // Limit number of items participating in shuffling
                    int count = Math.Min(learningItems[LearningExercise.Display].Count, itemsCount);
                    List<LearningItem> displayItems = new List<LearningItem>(shuffledDisplayItems).GetRange(0, count);

                    // Shuffle all lists
                    var keys = new List<LearningExercise>(learningItems.Keys);
                    foreach (var key in keys)
                    {
                        // Foreach exercise select only those items that overlap with displayItems
                        var items = learningItems[key].Intersect(displayItems, new LearningItemHashComparer());

                        // Shuffle and update
                        var shuffledItems = items.OrderBy(a => Guid.NewGuid());
                        learningItems[key] = new List<LearningItem>(shuffledItems);
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
                    case LearningExercise.EnglishSentence2Hanzi:
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
            // Black list
            List<LearningExercise> blackList = new List<LearningExercise>() {
                LearningExercise.English2Pinyin,
                LearningExercise.Pinyin2English,
                LearningExercise.Pinyin2Hanzi
            };

            List<LearningItem> allItems = new List<LearningItem>();
            foreach (var record in recordList)
            {
                foreach (LearningExercise exercise in ExerciseList)
                    if (!blackList.Contains(exercise))
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

            // If learning sentences then select only those words that have sentence defined
            List<LearningItem> selectedItems;
            if (Mode == LearningMode.Sentences)
                selectedItems = new List<LearningItem>(allItems.Where(i => i.Record.Sentence.Count == 2));
            else
                selectedItems = allItems;

            // Remove those words from display exercise that do not appear in other exercises
            List<LearningItem> reducedItems = new List<LearningItem>(selectedItems);
            foreach (var item in selectedItems)
            {
                if (item.Exercise == LearningExercise.Display)
                {
                    // If only one match found then remove (one match = Display exercise)
                    if (reducedItems.FindAll(i => i.Hash == item.Hash).Count == 1)
                        reducedItems.Remove(item);
                }
            }

            // Update learning list and return items count
            UpdateLearningList(reducedItems);

            if (Mode == LearningMode.Revision)
                return reducedItems.Count;
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

        public static void SetVisibility(out Visibility PinyinVisible, out Visibility TranslationVisible, out Visibility SimplifiedVisible, out Visibility SentenceVisible, out Visibility SentenceChineseVisible)
        {
            SentenceVisible = CurrentExercise == LearningExercise.Display ? Visibility.Visible : Visibility.Collapsed;
            SentenceChineseVisible = Visibility.Visible;

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
                case LearningExercise.DrawHanzi:
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
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Collapsed;
                    break;
                case LearningExercise.FillGapsEnglish:
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Visible;
                    break;
                case LearningExercise.EnglishSentence2Hanzi:
                    PinyinVisible = Visibility.Collapsed;
                    TranslationVisible = Visibility.Collapsed;
                    SimplifiedVisible = Visibility.Collapsed;
                    SentenceVisible = Visibility.Visible;
                    SentenceChineseVisible = Visibility.Collapsed;
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
            LearningItems = null;
            ResetExerciseState();
        }

        public static void ResetExerciseState()
        {
            currentItemIndex = 0;
            correctCount = 0;
            wrongCount = 0;
            lastResult = true;
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
            LearningItem curItem = LearningItems[CurrentExercise][currentItemIndex - 1];
            string status = "List: " + curItem.ListName + Environment.NewLine + Environment.NewLine;
#if DEBUG_ENGINE
            
            string time = curItem.Timestamp.ToString("dd-MM-yyyy");
            status += "#" + curItem.Score + " (" + time + ")" + Environment.NewLine;

            // Previous item
            if (currentItemIndex > 1)
            {
                LearningItem prevItem = LearningItems[CurrentExercise][currentItemIndex - 2];
                string pword = "(previous) Word: " + prevItem.Record.Chinese.Simplified + Environment.NewLine;
                string pleft = "(previous) Repetitions: " + prevItem.Score + Environment.NewLine;
                string pnext = "(previous) Timestamp: " + prevItem.Timestamp.ToString("dd-MM-yyyy") + Environment.NewLine;
                status += pword + pleft + pnext;
            }
#endif

            if (correctCount > 0 || wrongCount > 0)
            {
                int totalItems = LearningItems[CurrentExercise].Count;
                string score = "Total: " + ((int)(100.0 * Convert.ToDouble(correctCount) / Convert.ToDouble(totalItems))).ToString() + " %" + Environment.NewLine;
                string correct = "Correct: " + correctCount.ToString() + Environment.NewLine;
                string wrong = "Wrong: " + wrongCount.ToString() + Environment.NewLine;

                status += correct + wrong + score;
            }

            return status;
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

                        // Remove brackets and its content
                        string refText = Regex.Replace(s, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", String.Empty);

                        // Convert to lower-case and replace comas/dots/colons/brackets with spaces
                        refText = refText.ToLower().Replace(',',' ').Replace('.',' ').Replace(';',' ').Replace(':',' ').Replace("  "," ").Trim();

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
                case LearningExercise.DrawHanzi:
                    result = CurrentItem.Chinese.Simplified == inputText;
                    break;
                case LearningExercise.EnglishSentence2Hanzi:
                    result = CurrentItem.Sentence[0].Replace("。","") == inputText.Replace("。", "");
                    break;
                case LearningExercise.Hanzi2Pinyin:
                case LearningExercise.English2Pinyin:
                    // Convert to lower and remove spaces
                    string pinyin = CurrentItem.Chinese.PinyinNoMarkup.ToLower().Replace(" ", "");
                    inputText = inputText.Replace(" ", "");
                    result = pinyin == inputText;

                    // If failed then remove 5th tone notation and try again
                    if (!result)
                        result = pinyin.Replace("5", "") == inputText;

                    // If failed again then replace u: notation with v
                    if (!result)
                        result = pinyin.Replace("u:", "v") == inputText;
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

            lastResult = result;
            return result;
        }

        public static void RevertLastValidate(bool newResult = true)
        {
            if (CurrentExercise == LearningExercise.Display)
                return;

            if (newResult)
            {
                wrongCount--;
                correctCount++;
                LearningItems[CurrentExercise].RemoveAt(LearningItems[CurrentExercise].Count - 1);
            }
            else
            {
                wrongCount++;
                correctCount--;
                LearningItems[CurrentExercise].Add(LearningItems[CurrentExercise][currentItemIndex - 1]);
            }
            lastResult = newResult;

            // We need to update revision list twice in order to correct for the previous update
            RevisionEngine.UpdateRevisionList(LearningItems[CurrentExercise][currentItemIndex - 1], newResult, 2);
        }

        public static void MarkLastAsLearnt()
        {
            if (!lastResult)
                RevertLastValidate(true);

            RevisionEngine.UpdateRevisionList(LearningItems[CurrentExercise][currentItemIndex - 1], true, 10);
        }

        // Helper function to display enums description
        public static string GetDescription<T>(LearningExercise code) where T: IText
        {
            Type type = code.GetType();

            System.Reflection.MemberInfo[] memInfo = type.GetMember(code.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = (object[])memInfo[0].GetCustomAttributes(typeof(T), false);

                if (attrs != null && attrs.Length > 0)
                    return ((T)attrs[0]).GetText();
            }

            return code.ToString();
        }

        public static string CheckInputLanguage(string text)
        {
            // Give hint only when string has some characters
            if (String.IsNullOrWhiteSpace(text))
                return "";

            bool isLatin = System.Text.RegularExpressions.Regex.Match(
                text.Trim().Replace(" ", "").Replace("-", "").Replace("'","").Replace(".","").Replace(",","").Replace("?","").Replace("!",""),
                @"^[A-Za-z0-9]+$").Success;
            switch (CurrentExerciseList[CurrentExerciseIndex])
            {
                case LearningExercise.English2Hanzi:
                case LearningExercise.Pinyin2Hanzi:
                case LearningExercise.FillGapsChinese:
                case LearningExercise.EnglishSentence2Hanzi:
                    if (Windows.Globalization.Language.CurrentInputMethodLanguageTag.Contains("zh-") || !isLatin)
                        return "";
                    return "Change input method to Chinese";
                default:
                    return !isLatin ? "Change input method to English" : "";
            }
        }
    }
}
