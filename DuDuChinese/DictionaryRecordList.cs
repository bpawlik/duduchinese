using CC_CEDICT.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuDuChinese
{
    public class DictionaryRecordList : List<DictionaryRecord>
    {
        /// <summary>
        /// Indicates whether this list has been modified since it was loaded.
        /// </summary>
        /// <remarks>
        /// Effectively indicates whether the ListManager needs to save it to disk.
        /// </remarks>
        public bool IsModified = false;

        /// <summary>
        /// Indicates whether this list is marked as deleted.
        /// </summary>
        /// <remarks>
        /// Effectively indicates whether the ListManager should delete it from disk (i.e. when the app exits).
        /// </remarks>
        public bool IsDeleted = false;

        /// <summary>
        /// Indicates whether this list is a default
        /// </summary>
        public bool IsDefault = false;

        // metadata header
        public const string NameHeaderKey = "name";
        const string ReadOnlyHeaderKey = "readonly";
        const string SortOrderHeaderKey = "sortorder";

        /// <summary>
        /// Constructor for new lists where no dictionary file yet exists.
        /// </summary>
        /// <param name="name">The name of the list to be created.</param>
        public DictionaryRecordList(string name)
        {
            Name = name;
            SortOrder = ListSortOrder.Alphabetical;
        }

        /// <summary>
        /// Constructor for lists based on an existing dictionary file.
        /// </summary>
        /// <param name="dictionary">CC_CEDict.WindowsPhone instance containing the entries for the list.</param>
        public DictionaryRecordList(Dictionary dictionary)
        {
            try
            {
                // read headers
                Name = dictionary.Header[NameHeaderKey];
                if (dictionary.Header.ContainsKey(ReadOnlyHeaderKey))
                    ReadOnly = bool.Parse(dictionary.Header[ReadOnlyHeaderKey]);
                if (dictionary.Header.ContainsKey(SortOrderHeaderKey))
                    SortOrder = (ListSortOrder)Int32.Parse(dictionary.Header[SortOrderHeaderKey]);

                // read records
                foreach (DictionaryRecord record in dictionary)
                    base.Add(record);

                // release resource on disk
                dictionary.Dispose();
            }
            catch (Exception)
            {
            }

            IsModified = false;
        }

        string _Name = "unknown";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                IsModified = true;
            }
        }

        bool _ReadOnly = false;
        public bool ReadOnly
        {
            get
            {
                return _ReadOnly;
            }
            set
            {
                _ReadOnly = value;
                IsModified = true;
            }
        }

        public enum ListSortOrder { Alphabetical = 0, ReverseChronological = 1 };
        ListSortOrder _SortOrder = ListSortOrder.Alphabetical;
        public ListSortOrder SortOrder
        {
            get
            {
                return _SortOrder;
            }
            set
            {
                _SortOrder = value;
                IsModified = true;
            }
        }

        public new void Add(DictionaryRecord record)
        {
            if (this.Contains(record))
                return;
            base.Add(record);
            IsModified = true;
        }

        public new void Remove(DictionaryRecord record)
        {
            base.Remove(record);
            IsModified = true;
        }

        string HeaderTemplate = "#! {0}={1}";
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(HeaderTemplate, NameHeaderKey, this.Name));
            sb.AppendLine(String.Format(HeaderTemplate, ReadOnlyHeaderKey, this.ReadOnly));
            sb.AppendLine(String.Format(HeaderTemplate, SortOrderHeaderKey, (int)this.SortOrder));

            foreach (DictionaryRecord record in this)
                sb.AppendLine(record.ToString());

            return sb.ToString();
        }
    }
}
