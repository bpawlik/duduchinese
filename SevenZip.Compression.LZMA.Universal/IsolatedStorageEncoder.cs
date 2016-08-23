using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;

namespace SevenZip.Compression.LZMA.Universal
{
    public class IsolatedStorageEncoder : StreamEncoder
    {
        IsolatedStorageFile store = null;
        IsolatedStorageFileStream outStream;

        /// <summary>
        /// Extends StreamEncoder to decompress LZMA to a file in IsolatedStorage.
        /// </summary>
        /// <see cref="SevenZip.Compression.LZMA.Universal.StreamDecoder"/>
        public IsolatedStorageEncoder()
            : base()
        {
            RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(IsolatedStorageEncoder_RunWorkerCompleted);
        }

        /// <summary>
        /// Starts the file-to-file asynchronous LZMA compression operation.
        /// </summary>
        /// <param name="inFile">The path to the file from which to read the compressed data.</param>
        /// <param name="outFile">The path to the file (to be created) to which the decompressed data should be written.</param>
        /// <exception cref="System.IO.IsolatedStorage.IsolatedStorageException" />
        public void EncodeAsync(string inFile, string outFile)
        {
            store = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream inStream = new IsolatedStorageFileStream(inFile, FileMode.Open, store);
            EncodeAsync(inStream, outFile);
        }

        /// <summary>
        /// Starts the stream-to-file asynchronous LZMA compression operation.
        /// </summary>
        /// <param name="inStream">The stream from which to read the compressed data.</param>
        /// <param name="outFile">The path to the file (to be created) to which the decompressed data should be written.</param>
        /// <exception cref="System.IO.IsolatedStorage.IsolatedStorageException" />
        public void EncodeAsync(Stream inStream, string outFile)
        {
            if (store == null)
                store = IsolatedStorageFile.GetUserStoreForApplication();
            outStream = new IsolatedStorageFileStream(outFile, FileMode.Create, store);
            currentItem = outFile;
            EncodeAsync(inStream, outStream);
        }

        /// <summary>
        /// Overrides StreamDecoder.FreeSpaceRequired to extend the IsolatedStorage quota, if necessary.
        /// </summary>
        /// <see cref="SevenZip.Compression.LZMA.StreamDecoder.FreeSpaceRequired"/>
        protected override bool FreeSpaceRequired(long bytes)
        {
            //if (store.AvailableFreeSpace >= bytes)
            //    return true;
            //return store.IncreaseQuotaTo(store.Quota + bytes);

            return true;
        }

        /// <summary>
        /// Clean up resources on completion of compression.
        /// </summary>
        void IsolatedStorageEncoder_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (store != null)
                store.Dispose();
        }
    }
}
