using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace SevenZip.Compression.LZMA.Universal
{
    public class StreamEncoder : BackgroundWorker, ICodeProgress
    {
        Stream outStream;
        protected long outSize;
        protected string currentItem;

        /// <summary>
        /// Permit multiple instances to encode concurrently (default=true).
        /// </summary>
        public static bool AllowConcurrentDecoding = true;
        static AutoResetEvent concurrency = new AutoResetEvent(true);

        /// <summary>
        /// Encapsulates the compression function of the LZMA SDK; implemented as a BackgroundWorker.
        /// </summary>
        /// <remarks>Register your own ProgressChanged and RunWorkerCompleted event handlers, e.g.</remarks>
        /// <example>
        /// using SevenZip.Compression.LZMA.Universal;
        /// ...
        /// StreamEncoder encoder = new StreamEncoder();
        /// encoder.ProgressChanged += new ProgressChangedEventHandler(encoder_ProgressChanged);
        /// encoder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(encoder_RunWorkerCompleted);
        /// encoder.EncodeAsync(inStream, outStream);
        /// ...
        /// void encoder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        /// {
        ///     // TODO: do something with e.ProgressPercentage
        /// ...
        /// void encoder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        /// {
        ///     // TODO: if (e.Error) ... else ...
        /// ...
        /// </example>
        /// <see cref="System.ComponentModel.BackgroundWorker"/>
        public StreamEncoder()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = false;
            DoWork += new DoWorkEventHandler(encoder_DoWork);
        }

        /// <summary>
        /// Starts the stream-to-stream asynchronous LZMA compression operation.
        /// </summary>
        /// <param name="inStream">The System.IO.Stream from which to read the compressed data.</param>
        /// <param name="outStream">The System.IO.Stream to which the decompressed data should be written.</param>
        /// <remarks>Make sure your RunWorkerCompletedEventHandler is attached before calling this method.</remarks>
        /// <see cref="System.ComponentModel.BackgroundWorker.RunWorkerCompleted"/>
        public void EncodeAsync(Stream inStream, Stream outStream)
        {
            this.outStream = outStream;
            RunWorkerAsync(inStream);
        }

        /// <summary>
        /// Internal wrapper implementation for SevenZip.Compression.LZMA.Decoder.
        /// </summary>
        /// <exception cref="ArgumentException" />
        /// <exception cref="InsufficientFreeSpaceException" />
        /// <remarks>Implementation taken from LzmaAlone.cs (in the LZMA SDK).</remarks>
        void encoder_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!AllowConcurrentDecoding)
                concurrency.WaitOne(); // block until signaled

            StreamEncoder encoder = (StreamEncoder)sender;
            Stream inStream = (Stream)e.Argument;
            DateTime start = DateTime.Now;
            
            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new ArgumentException("LZMA input data is too short");

            Compression.LZMA.Encoder lzmaEncoder = new Compression.LZMA.Encoder();
            //lzmaEncoder.SetEncoderProperties(properties);

            outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                if (v < 0)
                    throw new ArgumentException("LZMA input data is empty/unreadable");
                outSize |= ((long)(byte)v) << (8 * i);
            }

            try
            {
                if (!encoder.FreeSpaceRequired(outSize))
                {
                    Exception blame = new Exception(String.Format("Sorry, {0} didn't explain what went wrong.", this.GetType()));
                    throw new InsufficientFreeSpaceException(blame); // unknown issue; name and shame :)
                }
            }
            catch (InsufficientFreeSpaceException)
            {
                throw; // rethrow; this is already of the correct type
            }
            catch (Exception ex)
            {
                throw new InsufficientFreeSpaceException(ex); // other known issue; convert to InsufficientFreeSpaceException
            }

            long compressedSize = inStream.Length - inStream.Position;
            lzmaEncoder.Code(inStream, outStream, compressedSize, outSize, this);

            TimeSpan elapsed = DateTime.Now - start;
            double speed = outSize / 1024 / elapsed.TotalSeconds;
            Debug.WriteLine(String.Format("LZMA compression took {0}s. for {1}(c.)/{2}(u.) bytes at {3}KB/s", elapsed.TotalSeconds, compressedSize, outSize, (int)speed));
            
            if (!AllowConcurrentDecoding)
                concurrency.Set(); // reset signal

            outStream.Flush();
            outStream.Dispose();
        }

        /// <summary>
        /// Thrown when compression would produce more data than can be handled by the target storage location.
        /// </summary>
        /// <remarks>Examine InnerException for the specific reason for failure.</remarks>
        public class InsufficientFreeSpaceException : Exception
        {
            public InsufficientFreeSpaceException(Exception innerException)
                : this("An error occurred while attempting to ensure sufficient space for decompression.", innerException)
            {
            }

            public InsufficientFreeSpaceException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Called after reading the LZMA header and before starting compression.
        /// </summary>
        /// <param name="outSize">Number of bytes that will be written to the outStream.</param>
        /// <remarks>Intended to be overridden by extending classes to handle free space requirements.</remarks>
        protected virtual bool FreeSpaceRequired(long outSize)
        {
            return true;
        }

        #region ICodeProgress interface
        /// <summary>
        /// ICodeProgress callback method, used internally by SevenZip.Compression.LZMA.Decoder. Don't call this from client code!
        /// </summary>
        /// <remarks>May be overridden by extending classes which perform multiple background tasks.</remarks>
        public virtual void SetProgress(long inProgress, long outProgress)
        {
            ReportProgress((int)(100 * outProgress / outSize), currentItem);
        }
        #endregion
    }
}
