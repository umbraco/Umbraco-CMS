using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Umbraco.Core
{
    /// <summary>
    /// Used to generate a string hash using crypto libraries over multiple objects
    /// </summary>
    /// <remarks>
    /// This should be used to generate a reliable hash that survives AppDomain restarts.
    /// This will use the crypto libs to generate the hash and will try to ensure that
    /// strings, etc... are not re-allocated so it's not consuming much memory.
    /// </remarks>
    internal class HashGenerator : DisposableObjectSlim
    {
        public HashGenerator()
        {
            _writer = new StreamWriter(_ms, Encoding.Unicode, 1024, leaveOpen: true);
        }

        private readonly MemoryStream _ms = new MemoryStream();
        private StreamWriter _writer;

        internal void AddInt(int i)
        {
            _writer.Write(i);
        }

        internal void AddLong(long i)
        {
            _writer.Write(i);
        }

        internal void AddObject(object o)
        {
            _writer.Write(o);
        }

        internal void AddDateTime(DateTime d)
        {
            _writer.Write(d.Ticks);
        }

        internal void AddString(string s)
        {
            if (s != null)
                _writer.Write(s);
        }

        internal void AddCaseInsensitiveString(string s)
        {
            //I've tried to no allocate a new string with this which can be done if we use the CompareInfo.GetSortKey method which will create a new
            //byte array that we can use to write to the output, however this also allocates new objects so i really don't think the performance
            //would be much different. In any case, I'll leave this here for reference. We could write the bytes out based on the sort key,
            //this is how we could deal with case insensitivity without allocating another string
            //for reference see: https://stackoverflow.com/a/10452967/694494
            //we could go a step further and s.Normalize() but we're not really dealing with crazy unicode with this class so far.

            if (s != null)
                _writer.Write(s.ToUpperInvariant());
        }

        internal void AddFileSystemItem(FileSystemInfo f)
        {
            //if it doesn't exist, don't proceed.
            if (f.Exists == false)
                return;

            AddCaseInsensitiveString(f.FullName);
            AddDateTime(f.CreationTimeUtc);
            AddDateTime(f.LastWriteTimeUtc);

            //check if it is a file or folder
            var fileInfo = f as FileInfo;
            if (fileInfo != null)
            {
                AddLong(fileInfo.Length);
            }

            var dirInfo = f as DirectoryInfo;
            if (dirInfo != null)
            {
                foreach (var d in dirInfo.GetFiles())
                {
                    AddFile(d);
                }
                foreach (var s in dirInfo.GetDirectories())
                {
                    AddFolder(s);
                }
            }
        }

        internal void AddFile(FileInfo f)
        {
            AddFileSystemItem(f);
        }

        internal void AddFolder(DirectoryInfo d)
        {
            AddFileSystemItem(d);
        }

        /// <summary>
        /// Returns the generated hash output of all added objects
        /// </summary>
        /// <returns></returns>
        internal string GenerateHash()
        {
            //flush,close,dispose the writer,then create a new one since it's possible to keep adding after GenerateHash is called.

            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
            _writer = new StreamWriter(_ms, Encoding.UTF8, 1024, leaveOpen: true);

            var hashType = CryptoConfig.AllowOnlyFipsAlgorithms ? "SHA1" : "MD5";

            //create an instance of the correct hashing provider based on the type passed in
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null) throw new InvalidOperationException("No hashing type found by name " + hashType);
            using (hasher)
            {
                var buffer = _ms.GetBuffer();
                //get the hashed values created by our selected provider
                var hashedByteArray = hasher.ComputeHash(buffer);

                //create a StringBuilder object
                var stringBuilder = new StringBuilder();

                //loop to each byte
                foreach (var b in hashedByteArray)
                {
                    //append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2"));
                }

                //return the hashed value
                return stringBuilder.ToString();
            }
        }

        protected override void DisposeResources()
        {
            _writer.Close();
            _writer.Dispose();
            _ms.Close();
            _ms.Dispose();
        }
    }
}
