using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Search;
using Umbraco.Core;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    public class ExamineManagementApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Get the details for indexers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<ExamineIndexerModel> GetIndexerDetails()
        {
            return ExamineManager.Instance.IndexProviderCollection.Select(CreateModel);
        }

        /// <summary>
        /// Get the details for searchers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<ExamineSearcherModel> GetSearcherDetails()
        {
            var model = new List<ExamineSearcherModel>(
                ExamineManager.Instance.SearchProviderCollection.Cast<BaseSearchProvider>().Select(searcher =>
                {
                    var indexerModel = new ExamineIndexerModel()
                    {
                        Name = searcher.Name
                    };
                    var props = TypeHelper.CachedDiscoverableProperties(searcher.GetType(), mustWrite: false)
                        //ignore these properties
                                          .Where(x => !new[] { "Description" }.InvariantContains(x.Name))
                                          .OrderBy(x => x.Name);
                    foreach (var p in props)
                    {
                        indexerModel.ProviderProperties.Add(p.Name, p.GetValue(searcher, null).ToString());
                    }
                    return indexerModel;
                }));
            return model;
        }

        /// <summary>
        /// Optimizes an index
        /// </summary>
        [HttpPost]
        public HttpResponseMessage PostOptimizeIndex(string indexerName)
        {
            LuceneIndexer indexer;
            var msg = ValidateLuceneIndexer(indexerName, out indexer);
            if (msg.IsSuccessStatusCode)
            {
                try
                {
                    indexer.OptimizeIndex();
                }
                catch (System.Exception ex)
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict)
                    {
                        Content = new StringContent(string.Format("The index could not be optimized, most likely there is another thread currently writing to the index. Error: {0}", ex)),
                        ReasonPhrase = "Could Not Optimize"
                    };
                }
            }
            return msg;
        }

        /// <summary>
        /// Checks if the index is optimized
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        [HttpPost]
        public ExamineIndexerModel PostCheckOptimizeIndex(string indexerName)
        {
            LuceneIndexer indexer;
            var msg = ValidateLuceneIndexer(indexerName, out indexer);
            if (msg.IsSuccessStatusCode)
            {
                var isOptimized = indexer.IsIndexOptimized();
                return !isOptimized
                    ? null
                    : CreateModel(indexer);
            }
            throw new HttpResponseException(msg);
        }

        private ExamineIndexerModel CreateModel(BaseIndexProvider indexer)
        {
            var indexerModel = new ExamineIndexerModel()
            {
                IndexCriteria = indexer.IndexerData,
                Name = indexer.Name
            };
            var props = TypeHelper.CachedDiscoverableProperties(indexer.GetType(), mustWrite: false)
                //ignore these properties
                                  .Where(x => !new[] { "IndexerData", "Description", "WorkingFolder" }.InvariantContains(x.Name))
                                  .OrderBy(x => x.Name);
            foreach (var p in props)
            {
                indexerModel.ProviderProperties.Add(p.Name, p.GetValue(indexer, null).ToString());
            }

            var luceneIndexer = indexer as LuceneIndexer;
            if (luceneIndexer != null && luceneIndexer.IndexExists())
            {                
                indexerModel.IsLuceneIndex = true;
                indexerModel.DocumentCount = luceneIndexer.GetIndexDocumentCount();
                indexerModel.FieldCount = luceneIndexer.GetIndexDocumentCount();
                indexerModel.IsOptimized = luceneIndexer.IsIndexOptimized();
                indexerModel.DeletionCount = luceneIndexer.GetDeletedDocumentsCount();                
            }
            return indexerModel;
        }

        private HttpResponseMessage ValidateLuceneIndexer(string indexerName, out LuceneIndexer indexer)
        {            
            if (ExamineManager.Instance.IndexProviderCollection.Any(x => x.Name == indexerName))
            {
                indexer = ExamineManager.Instance.IndexProviderCollection[indexerName] as LuceneIndexer;
                if (indexer == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(string.Format("The indexer {0} is not of type {1}", indexerName, typeof(LuceneIndexer))),
                        ReasonPhrase = "Wrong Indexer Type"
                    };
                }                
                //return Ok!
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            indexer = null;
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("No indexer found with name = {0}", indexerName)),
                    ReasonPhrase = "Indexer Not Found"
                };
        }
    }
}
