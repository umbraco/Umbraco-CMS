using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine;
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
        public IEnumerable<ExamineIndexerModel> GetIndexerDetails()
        {
            return ExamineManager.Instance.IndexProviderCollection.Select(CreateModel);
        }

        /// <summary>
        /// Get the details for searchers
        /// </summary>
        /// <returns></returns>
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

        public ISearchResults GetSearchResults(string searcherName, string query, string queryType)
        {
            if (queryType == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
                

            if (query.IsNullOrWhiteSpace())
                return SearchResults.Empty();

            LuceneSearcher searcher;
            var msg = ValidateLuceneSearcher(searcherName, out searcher);
            if (msg.IsSuccessStatusCode)
            {
                if (queryType.InvariantEquals("text"))
                {
                    return searcher.Search(query, false);
                }
                if (queryType.InvariantEquals("lucene"))
                {
                    return searcher.Search(searcher.CreateSearchCriteria().RawQuery(query));
                }
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            throw new HttpResponseException(msg);            
        }

        /// <summary>
        /// Optimizes an index
        /// </summary>
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
                catch (Exception ex)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict);
                    response.Content = new StringContent(string.Format("The index could not be optimized, most likely there is another thread currently writing to the index. Error: {0}", ex));
                    response.ReasonPhrase = "Could Not Optimize";
                    return response;
                }
            }
            return msg;
        }

        /// <summary>
        /// Rebuilds the index
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        public HttpResponseMessage PostRebuildIndex(string indexerName)
        {
            LuceneIndexer indexer;
            var msg = ValidateLuceneIndexer(indexerName, out indexer);
            if (msg.IsSuccessStatusCode)
            {
                try
                {
                    indexer.RebuildIndex();
                }
                catch (Exception ex)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict);
                    response.Content = new StringContent(string.Format("The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {0}", ex));
                    response.ReasonPhrase = "Could Not Rebuild";
                    return response;
                }
            }
            return msg;
        }

        /// <summary>
        /// Check if the index has been rebuilt
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is kind of rudementary since there's no way we can know that the index has rebuilt, we'll just check
        /// if the index is locked based on Lucene apis
        /// </remarks>
        public ExamineIndexerModel PostCheckRebuildIndex(string indexerName)
        {
            LuceneIndexer indexer;
            var msg = ValidateLuceneIndexer(indexerName, out indexer);
            if (msg.IsSuccessStatusCode)
            {
                var isLocked = indexer.IsIndexLocked();
                return isLocked
                    ? null
                    : CreateModel(indexer);
            }
            throw new HttpResponseException(msg);
        }

        /// <summary>
        /// Checks if the index is optimized
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
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

        private HttpResponseMessage ValidateLuceneSearcher(string searcherName, out LuceneSearcher searcher)
        {
            if (ExamineManager.Instance.SearchProviderCollection.Cast<BaseSearchProvider>().Any(x => x.Name == searcherName))
            {
                searcher = ExamineManager.Instance.SearchProviderCollection[searcherName] as LuceneSearcher;
                if (searcher == null)
                {
                    var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.Content = new StringContent(string.Format("The searcher {0} is not of type {1}", searcherName, typeof(LuceneSearcher)));
                    response.ReasonPhrase = "Wrong Searcher Type";
                    return response;
                }
                //return Ok!
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            searcher = null;

            var response1 = Request.CreateResponse(HttpStatusCode.BadRequest);
            response1.Content = new StringContent(string.Format("No searcher found with name = {0}", searcherName));
            response1.ReasonPhrase = "Searcher Not Found";
            return response1;
        }

        private HttpResponseMessage ValidateLuceneIndexer(string indexerName, out LuceneIndexer indexer)
        {            
            if (ExamineManager.Instance.IndexProviderCollection.Any(x => x.Name == indexerName))
            {
                indexer = ExamineManager.Instance.IndexProviderCollection[indexerName] as LuceneIndexer;
                if (indexer == null)
                {
                    var response1 = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response1.Content = new StringContent(string.Format("The indexer {0} is not of type {1}", indexerName, typeof(LuceneIndexer)));
                    response1.ReasonPhrase = "Wrong Indexer Type";
                    return response1;
                }                
                //return Ok!
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            indexer = null;

            var response = Request.CreateResponse(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Format("No indexer found with name = {0}", indexerName));
            response.ReasonPhrase = "Indexer Not Found";
            return response;
        }
    }
}
