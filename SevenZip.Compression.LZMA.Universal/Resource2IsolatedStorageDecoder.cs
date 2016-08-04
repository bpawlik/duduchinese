﻿using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Windows.Storage;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Resources;
//using System.Windows.Shapes;

namespace SevenZip.Compression.LZMA.WindowsPhone
{
    public class Resource2IsolatedStorageDecoder : IsolatedStorageDecoder
    {
        /// <summary>
        /// Starts the resource-to-file asynchronous LZMA decompression operation.
        /// </summary>
        /// <param name="inFile">The path to the embedded resource from which to read the compressed data.</param>
        /// <param name="outFile">The path to the file (to be created) to which the decompressed data should be written.</param>
        /// <remarks>
        /// This method assumes the compressed resource in the same assembly as your calling code.
        /// If your compressed resource is elsewhere, see DecodeAsync(Uri inUri, string outFile).
        /// </remarks>
        /// <see cref="Resource2IsolatedStorageDecoder.DecodeAsync(Uri inUri, string outFile)"/>
        public new void DecodeAsync(string inFile, string outFile)
        {
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            //Uri uri = new Uri(String.Format("{0}.{1}", assemblyName.Name, inFile), UriKind.Relative);
            //DecodeAsync(uri, outFile);

            //AssemblyName assemblyName = new AssemblyName(System.Reflection.Assembly.GetCallingAssembly().FullName);
            //Uri uri = new Uri(String.Format("/{0};component/{1}", assemblyName.Name, inFile), UriKind.Relative);
            //DecodeAsync(uri, outFile);

            Stream resourceStream = assembly.GetManifestResourceStream("DuDuChinese"+ ".Assets." + inFile);

            var names = this.GetType()
             .GetTypeInfo()
             .Assembly
             .GetManifestResourceNames();

            currentItem = outFile;
            DecodeAsync(resourceStream, outFile);
        }

        /// <summary>
        /// Starts the resource URI-to-file asynchronous download and decompression operation.
        /// </summary>
        /// <param name="inUri">The URI to the embedded resource from which to read the compressed data.</param>
        /// <param name="outFile">The path to the file (to be created) to which the decompressed data should be written.</param>
        /// <remarks>Use this method if you need to explicitly specify the assembly/resource location as a Uri.</remarks>
        public void DecodeAsync(Uri inUri, string outFile)
        {
            //StreamResourceInfo resource = Application.GetResourceStream(inUri);
            //currentItem = outFile;
            //DecodeAsync(resource.Stream, outFile);

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream(inUri.AbsoluteUri);
            currentItem = outFile;
            DecodeAsync(resourceStream, outFile);
        }
    }
}
