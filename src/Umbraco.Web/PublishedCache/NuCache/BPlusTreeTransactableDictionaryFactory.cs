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
        private readonly ISerializer<ContentNodeKit> _contentNodeKitSerializer;

        public BPlusTreeTransactableDictionaryFactory(ISerializer<ContentNodeKit> contentNodeKitSerializer = null)
        {
            _contentNodeKitSerializer = contentNodeKitSerializer;
        }
        public ITransactableDictionary<int, ContentNodeKit> Create(string filepath, bool exists)
        {
            return new BPlusTreeTransactableDictionary<int, ContentNodeKit>(BTree.GetTree(filepath, exists, _contentNodeKitSerializer), filepath);
        }
    }
}
