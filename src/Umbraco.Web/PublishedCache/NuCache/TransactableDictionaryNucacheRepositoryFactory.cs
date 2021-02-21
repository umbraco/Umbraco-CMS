using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants = Umbraco.Core.Constants;
namespace Umbraco.Web.PublishedCache.NuCache
{
    public class TransactableDictionaryNucacheRepositoryFactory : INucacheRepositoryFactory
    { 
        private readonly ITransactableDictionaryFactory<int, ContentNodeKit> _transactableDictionaryFactory;

        public TransactableDictionaryNucacheRepositoryFactory(ITransactableDictionaryFactory<int,ContentNodeKit> transactableDictionaryFactory)
        {
            _transactableDictionaryFactory = transactableDictionaryFactory;
        }
        public INucacheContentRepository GetContentRepository()
        {
            var transactableDictionary = _transactableDictionaryFactory.Get(Constants.NuCache.ContentDatabaseName);
            return new TransactableDictionaryNucacheRepository(transactableDictionary);
        }

        public INucacheMediaRepository GetMediaRepository()
        {
            var transactableDictionary = _transactableDictionaryFactory.Get(Constants.NuCache.MediaDatabaseName);
            return new TransactableDictionaryNucacheRepository(transactableDictionary);
        }
    }
}
