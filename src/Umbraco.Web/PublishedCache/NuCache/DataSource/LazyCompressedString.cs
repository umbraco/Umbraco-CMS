using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public class LazyCompressedString
    {
        private byte[] _bytes;
        private string _str;

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
