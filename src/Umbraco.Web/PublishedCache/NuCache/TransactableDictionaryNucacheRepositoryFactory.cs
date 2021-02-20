using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class TransactableDictionaryNucacheRepositoryFactory : INucacheRepositoryFactory
    { 
        private readonly ITransactableDictionaryFactory _transactableDictionaryFactory;

        public TransactableDictionaryNucacheRepositoryFactory(ITransactableDictionaryFactory transactableDictionaryFactory)
        {
            _transactableDictionaryFactory = transactableDictionaryFactory;
        }
        public INucacheDocumentRepository GetDocumentRepository()
        {
            var transactableDictionary = _transactableDictionaryFactory.Get(ContentCacheEntityType.Document);
            return new TransactableDictionaryNucacheRepository(transactableDictionary);
        }

        public INucacheMediaRepository GetMediaRepository()
        {
            var transactableDictionary = _transactableDictionaryFactory.Get(ContentCacheEntityType.Media);
            return new TransactableDictionaryNucacheRepository(transactableDictionary);
        }
    }
}
