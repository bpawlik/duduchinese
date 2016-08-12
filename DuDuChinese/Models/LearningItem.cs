using CC_CEDICT.Universal;
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
        public DictionaryRecord Record { get; set; }

        [DataMember]
        public string Hash { get; private set; }

        [DataMember]
        public string ListName;

        [DataMember]
        public LearningExercise Exercise;

        [DataMember]
        public int Score = 10;

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
