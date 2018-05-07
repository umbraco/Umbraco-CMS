using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class BTree
    {
        public static BPlusTree<int, ContentNodeKit> GetTree(string filepath, bool exists)
        {
            var keySerializer = new PrimitiveSerializer();
            var valueSerializer = new ContentNodeKitSerializer();
            var options = new BPlusTree<int, ContentNodeKit>.OptionsV2(keySerializer, valueSerializer)
            {
                CreateFile = exists ? CreatePolicy.IfNeeded : CreatePolicy.Always,
                FileName = filepath,

                // other options?
            };

            var tree = new BPlusTree<int, ContentNodeKit>(options);

            // anything?
            //btree.

            return tree;
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
