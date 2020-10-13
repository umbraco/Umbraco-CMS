using System.Configuration;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class BTree
    {
        public static BPlusTree<int, ContentNodeKit> GetTree(string filepath, bool exists, NuCacheSettings settings)
        {
            var keySerializer = new PrimitiveSerializer();
            var valueSerializer = new ContentNodeKitSerializer();
            var options = new BPlusTree<int, ContentNodeKit>.OptionsV2(keySerializer, valueSerializer)
            {
                CreateFile = exists ? CreatePolicy.IfNeeded : CreatePolicy.Always,
                FileName = filepath,

                // read or write but do *not* keep in memory
                CachePolicy = CachePolicy.None,

                // default is 4096, min 2^9 = 512, max 2^16 = 64K
                FileBlockSize = GetBlockSize(settings),

                //HACK: Forces FileOptions to be WriteThrough here: https://github.com/mamift/CSharpTest.Net.Collections/blob/9f93733b3af7ee0e2de353e822ff54d908209b0b/src/CSharpTest.Net.Collections/IO/TransactedCompoundFile.cs#L316-L327,
                // as the reflection uses otherwise will failed in .NET Core as the "_handle" field in FileStream is renamed to "_fileHandle".
                StoragePerformance = StoragePerformance.CommitToDisk,



                // other options?
            };

            var tree = new BPlusTree<int, ContentNodeKit>(options);

            // anything?
            //btree.

            return tree;
        }

        private static int GetBlockSize(NuCacheSettings settings)
        {
            var blockSize = 4096;

            var appSetting = settings.BTreeBlockSize;
            if (!appSetting.HasValue)
                return blockSize;

            blockSize = appSetting.Value;

            var bit = 0;
            for (var i = blockSize; i != 1; i >>= 1)
                bit++;
            if (1 << bit != blockSize)
                throw new ConfigurationErrorsException($"Invalid block size value \"{blockSize}\": must be a power of two.");
            if (blockSize < 512 || blockSize > 65536)
                throw new ConfigurationErrorsException($"Invalid block size value \"{blockSize}\": must be >= 512 and <= 65536.");

            return blockSize;
        }

        /*
        class ListOfIntSerializer : ISerializer<List<int>>
        {
            public List<int> ReadFrom(Stream stream)
            {
                var list = new List<int>();
                var count = PrimitiveSerializer.Int32.ReadFrom(stream);
                for (var i = 0; i < count; i++)
                    list.Add(PrimitiveSerializer.Int32.ReadFrom(stream));
                return list;
            }

            public void WriteTo(List<int> value, Stream stream)
            {
                PrimitiveSerializer.Int32.WriteTo(value.Count, stream);
                foreach (var item in value)
                    PrimitiveSerializer.Int32.WriteTo(item, stream);
            }
        }
        */
    }
}
