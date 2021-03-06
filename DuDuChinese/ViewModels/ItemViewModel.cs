﻿using Template10.Mvvm;
using CC_CEDICT.Universal;
using Windows.UI.Xaml;
using System;

namespace DuDuChinese.ViewModels
{
    public class ItemViewModel : ViewModelBase
    {
        public DictionaryRecord Record;

        private string _pinyin;
        public string Pinyin
        {
            get
            {
                return _pinyin;
            }
            set
            {
                if (value != _pinyin)
                {
                    Set(ref this._pinyin, value);
                }
            }
        }

        private string _english;
        public string English
        {
            get
            {
                return _english;
            }
            set
            {
                if (value != _english)
                {
                    Set(ref this._english, value);
                }
            }
        }

        private string _englishWithNewlines;
        public string EnglishWithNewlines
        {
            get
            {
                return _englishWithNewlines;
            }
            set
            {
                if (value != _englishWithNewlines)
                {
                    Set(ref this._englishWithNewlines, value);
                }
            }
        }

        private string _chinese;
        public string Chinese
        {
            get
            {
                return _chinese;
            }
            set
            {
                if (value != _chinese)
                {
                    Set(ref this._chinese, value);
                }
            }
        }

        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value != _index)
                {
                    Set(ref this._index, value);
                }
            }
        }

        private string sentence;
        public string Sentence
        {
            get { return this.sentence; }
            set { Set(ref this.sentence, value); }
        }

        private string sentenceChinese;
        public string SentenceChinese
        {
            get { return this.sentenceChinese; }
            set { Set(ref this.sentenceChinese, value); }
        }

        private string sentencePinyin;
        public string SentencePinyin
        {
            get { return this.sentencePinyin; }
            set { Set(ref this.sentencePinyin, value); }
        }

        private string sentenceEnglish;
        public string SentenceEnglish
        {
            get { return this.sentenceEnglish; }
            set { Set(ref this.sentenceEnglish, value); }
        }

        #region Visibility flags

        private Visibility pinyinVisible = Visibility.Collapsed;
        public Visibility PinyinVisible
        {
            get { return this.pinyinVisible; }
            set { this.Set(ref this.pinyinVisible, value); }
        }

        private Visibility translationVisible = Visibility.Collapsed;
        public Visibility TranslationVisible
        {
            get { return this.translationVisible; }
            set { this.Set(ref this.translationVisible, value); }
        }

        private Visibility simplifiedVisible = Visibility.Collapsed;
        public Visibility SimplifiedVisible
        {
            get { return this.simplifiedVisible; }
            set { this.Set(ref this.simplifiedVisible, value); }
        }

        private Visibility sentenceVisible = Visibility.Collapsed;
        public Visibility SentenceVisible
        {
            get {
                if (String.IsNullOrWhiteSpace(this.Sentence))
                    this.sentenceVisible = Visibility.Collapsed;
                return this.sentenceVisible;
            }
            set {
                if (String.IsNullOrWhiteSpace(this.Sentence))
                    this.sentenceVisible = Visibility.Collapsed;
                this.Set(ref this.sentenceVisible, value);
            }
        }

        private Visibility sentenceChineseVisible = Visibility.Collapsed;
        public Visibility SentenceChineseVisible
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this.SentenceChinese))
                    this.sentenceChineseVisible = Visibility.Collapsed;
                return this.sentenceChineseVisible;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(this.SentenceChinese))
                    this.sentenceChineseVisible = Visibility.Collapsed;
                this.Set(ref this.sentenceChineseVisible, value);
            }
        }

        #endregion
    }
}
