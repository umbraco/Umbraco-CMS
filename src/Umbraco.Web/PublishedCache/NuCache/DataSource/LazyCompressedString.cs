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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bytes">LZ4 Pickle compressed UTF8 String</param>
        public LazyCompressedString(byte[] bytes)
        {
            _locker = new object();
            _bytes = bytes;
            _str = null;            
        }

        public byte[] GetBytes()
        {
            if (_bytes == null)
                throw new InvalidOperationException("The bytes have already been expanded");
            return _bytes;
        }

        public override string ToString()
        {
            if (_str != null) return _str;
            lock (_locker)
            {
                if (_str != null) return _str; // double check
                if (_bytes == null) throw new PanicException("Bytes have already been cleared");
                _str = Encoding.UTF8.GetString(LZ4Pickler.Unpickle(_bytes));
                _bytes = null;
            }
            return _str;
        }

        public static implicit operator string(LazyCompressedString l) => l.ToString();
    }

}
