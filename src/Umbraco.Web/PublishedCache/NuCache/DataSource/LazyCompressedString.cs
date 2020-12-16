using K4os.Compression.LZ4;
using System;
using System.Text;
using Umbraco.Core.Exceptions;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Lazily decompresses a LZ4 Pickler compressed UTF8 string
    /// </summary>
    internal struct LazyCompressedString
    {
        private byte[] _bytes;
        private string _str;
        private readonly object _locker;
        private bool _isDecompressed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bytes">LZ4 Pickle compressed UTF8 String</param>
        public LazyCompressedString(byte[] bytes)
        {
            _locker = new object();
            _bytes = bytes;
            _str = null;
            _isDecompressed = false;
        }

        public byte[] GetBytes()
        {
            if (_bytes == null)
                throw new InvalidOperationException("The bytes have already been expanded");
            return _bytes;
        }
        /// <summary>
        /// Whether the bytes have been decompressed to a string. If true calling GetBytes() will throw InvalidOperationException.
        /// </summary>
        public bool IsDecompressed => _isDecompressed;
        

        public override string ToString()
        {
            if (_str != null) return _str;
            lock (_locker)
            {
                if (_str != null) return _str; // double check
                if (_bytes == null) throw new PanicException("Bytes have already been cleared");
                _str = Encoding.UTF8.GetString(LZ4Pickler.Unpickle(_bytes));
                _bytes = null;
                _isDecompressed = true;
            }
            return _str;
        }

        public static implicit operator string(LazyCompressedString l) => l.ToString();
    }

}
