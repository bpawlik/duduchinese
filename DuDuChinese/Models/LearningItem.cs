﻿using CC_CEDICT.Universal;
using System;
using System.Runtime.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DuDuChinese.Models
{
    [DataContract]
    public class LearningItem
    {
        private DictionaryRecord record = null;
        public DictionaryRecord Record {
            get
            {
                if (this.record == null && this.ListName != null && this.Hash != null)
                {
                    App app = (App)Windows.UI.Xaml.Application.Current;
                    if (app.ListManager.ContainsKey(this.ListName))
                        this.record = app.ListManager[this.ListName].Find(r => LearningItem.ComputeHash(r) == this.Hash);
                }
                return this.record;

            }
            set { this.record = value; }
        }

        [DataMember]
        public string Hash { get; private set; } = null;

        [DataMember]
        public string ListName { get; set; } = null;

        [DataMember]
        public LearningExercise Exercise { get; set; }

        [DataMember]
        public int Score { get; set; } = 10;

        public LearningItem(DictionaryRecord record)
        {
            this.Record = record;
            this.Hash = ComputeHash(record);
        }

        public static string ComputeHash(DictionaryRecord record)
        {
            return ComputeHash(record.Chinese.PinyinNoMarkup + record.Chinese.Simplified);
        }

        private static string ComputeHash(string strMsg)
        {
            // Convert the message string to binary data.
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            // Hash the message.
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);

            // Verify that the hash length equals the length specified for the algorithm.
            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            // Convert the hash to a string (for display).
            string strHashBase64 = CryptographicBuffer.EncodeToBase64String(buffHash);

            // Return the encoded string
            return strHashBase64;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object other)
        {
            if (other is LearningItem)
            {
                LearningItem r = (LearningItem)other;
                return r.Hash == this.Hash && r.Exercise == this.Exercise && r.ListName == this.ListName;
            }

            return false;
        }
    }
}