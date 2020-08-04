using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Lazily decompresses a LZ4 Pickler compressed UTF8 string
    /// </summary>
    internal class LazyCompressedString
    {
        private byte[] _bytes;
        private string _str;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bytes">LZ4 Pickle compressed UTF8 String</param>
        public LazyCompressedString(byte[] bytes)
        {
            _bytes = bytes;
        }

        public override string ToString()
        {
            return LazyInitializer.EnsureInitialized(ref _str, () =>
            {
                var str = Encoding.UTF8.GetString(LZ4Pickler.Unpickle(_bytes));
                _bytes = null;
                return str;
            });
        }
    }

}
