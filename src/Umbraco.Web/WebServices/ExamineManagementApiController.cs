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
using Umbraco.Core.Logging;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebServices
{
    [ValidateAngularAntiForgeryToken]
    public class ExamineManagementApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Checks if the member internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckMembersInternalIndex()
        {
            var total = Services.MemberService.Count();

            var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"]
                .CreateSearchCriteria().RawQuery("__IndexType:member");
            var totalIndexed = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].Search(criteria);

            return total == totalIndexed.TotalItemCount;
        }

        /// <summary>
        /// Checks if the media internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckMediaInternalIndex()
        {
            var total = Services.MediaService.Count();

            var criteria = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"]
                .CreateSearchCriteria().RawQuery("__IndexType:media");
            var totalIndexed = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"].Search(criteria);

            return total == totalIndexed.TotalItemCount;
        }

        /// <summary>
        /// Checks if the content internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckContentInternalIndex()
        {
            var total = Services.ContentService.Count();

            var criteria = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"]
                .CreateSearchCriteria().RawQuery("__IndexType:content");
            var totalIndexed = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"].Search(criteria);

            return total == totalIndexed.TotalItemCount;
        }

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
                                          .Where(x => new[] {"Description"}.InvariantContains(x.Name) == false)
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
                //remove it in case there's a handler there alraedy
                indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
                //now add a single handler
                indexer.IndexOperationComplete += Indexer_IndexOperationComplete;

                var cacheKey = "temp_indexing_op_" + indexer.Name;
                //put temp val in cache which is used as a rudimentary way to know when the indexing is done
                ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5), isSliding: false);

                try
                {
                    indexer.RebuildIndex();
                }
                catch (Exception ex)
                {
                    //ensure it's not listening
                    indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
                    LogHelper.Error<ExamineManagementApiController>("An error occurred rebuilding index", ex);
                    var response = Request.CreateResponse(HttpStatusCode.Conflict);
                    response.Content = new StringContent(string.Format("The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {0}", ex));
                    response.ReasonPhrase = "Could Not Rebuild";
                    return response;
                }
            }
            return msg;
        }

        //static listener so it's not GC'd
        private static void Indexer_IndexOperationComplete(object sender, EventArgs e)
        {
            var indexer = (LuceneIndexer) sender;

            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;

            var cacheKey = "temp_indexing_op_" + indexer.Name;
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(cacheKey);
        }

        /// <summary>
        /// Check if the index has been rebuilt
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is kind of rudimentary since there's no way we can know that the index has rebuilt, we 
        /// have a listener for the index op complete so we'll just check if that key is no longer there in the runtime cache
        /// </remarks>
        public ExamineIndexerModel PostCheckRebuildIndex(string indexerName)
        {
            LuceneIndexer indexer;
            var msg = ValidateLuceneIndexer(indexerName, out indexer);
            if (msg.IsSuccessStatusCode)
            {
                var cacheKey = "temp_indexing_op_" + indexerName;
                var found = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem(cacheKey);                
                //if its still there then it's not done
                return found != null
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
                                  .Where(x => new[] {"IndexerData", "Description", "WorkingFolder"}.InvariantContains(x.Name) == false)
                                  .OrderBy(x => x.Name);
								  
            foreach (var p in props)
            {
                var val = p.GetValue(indexer, null);
                if (val == null)
                {
                    LogHelper.Warn<ExamineManagementApiController>("Property value was null when setting up property on indexer: " + indexer.Name + " property: " + p.Name);
                    val = string.Empty;
                }
                indexerModel.ProviderProperties.Add(p.Name, val.ToString());
            }

            var luceneIndexer = indexer as LuceneIndexer;
            if (luceneIndexer != null)
            {                
                indexerModel.IsLuceneIndex = true;

                if (luceneIndexer.IndexExists())
                {
                    indexerModel.DocumentCount = luceneIndexer.GetIndexDocumentCount();
                    indexerModel.FieldCount = luceneIndexer.GetIndexFieldCount();
                    indexerModel.IsOptimized = luceneIndexer.IsIndexOptimized();
                    indexerModel.DeletionCount = luceneIndexer.GetDeletedDocumentsCount();
                }
                else
                {
                    indexerModel.DocumentCount = 0;
                    indexerModel.FieldCount = 0;
                    indexerModel.IsOptimized = true;
                    indexerModel.DeletionCount = 0;
                }
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
