using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class BPlusTreeTransactableDictionaryFactory : ITransactableDictionaryFactory
    {
        private readonly ISerializer<ContentData> _contentDataSerializer;

        public BPlusTreeTransactableDictionaryFactory(ISerializer<ContentData> contentDataSerializer = null)
        {
            _contentDataSerializer = contentDataSerializer;
        }
        public ITransactableDictionary<int, ContentNodeKit> Get(string filepath, bool exists)
        {
            return new BPlusTreeTransactableDictionary<int, ContentNodeKit>(BTree.GetTree(filepath, exists, _contentDataSerializer));
        }
    }
}
