using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.Search;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class ExamineManagementController : UmbracoAuthorizedJsonController
    {
        private readonly IExamineManager _examineManager;
        private readonly ILogger _logger;
        private readonly IRuntimeCacheProvider _runtimeCacheProvider;

        public ExamineManagementController(IExamineManager examineManager, ILogger logger,
                                           IRuntimeCacheProvider runtimeCacheProvider)
        {
            _examineManager = examineManager;
            _logger = logger;
            _runtimeCacheProvider = runtimeCacheProvider;
        }

        /// <summary>
        /// Get the details for indexers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineIndexerModel> GetIndexerDetails()
        {
            return _examineManager.IndexProviders.Select(CreateModel).OrderBy(x =>
            {
                //order by name , but strip the "Indexer" from the end if it exists
                return x.Name.TrimEnd("Indexer");
            });
        }

        /// <summary>
        /// Get the details for searchers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineSearcherModel> GetSearcherDetails()
        {
            var model = new List<ExamineSearcherModel>(
               _examineManager.IndexProviders.Select(indexer =>
               {
                   var searcher = indexer.Value.GetSearcher();
                   var searcherName = (searcher as BaseLuceneSearcher)?.Name ?? string.Concat(indexer.Key, "Searcher");

                   var indexerModel = new ExamineSearcherModel
                   {
                       Name = searcherName
                   };
                   var props = TypeHelper.CachedDiscoverableProperties(searcher.GetType(), mustWrite: false)
                               //ignore these properties
                               .Where(x => new[] { "Description" }.InvariantContains(x.Name) == false)
                               .Where(x => x.GetCustomAttribute<EditorBrowsableAttribute>()
                                           ?.State != EditorBrowsableState.Never)
                               .OrderBy(x => x.Name);
                   foreach (var p in props)
                   {
                       indexerModel.ProviderProperties.Add(p.Name, p.GetValue(searcher, null)?.ToString());
                   }

                   return indexerModel;
               }).OrderBy(x =>
               {
                   //order by name , but strip the "Searcher" from the end if it exists
                   return x.Name.TrimEnd("Searcher");
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
            {
                return LuceneSearchResults.Empty();
            }

            var msg = ValidateLuceneSearcher(searcherName, out var searcher);
            if (msg.IsSuccessStatusCode)
            {
                if (queryType.InvariantEquals("text"))
                {
                    return searcher.Search(query, false);
                }

                if (queryType.InvariantEquals("lucene"))
                {
                    return searcher.Search(searcher.CreateCriteria().RawQuery(query));
                }

                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            throw new HttpResponseException(msg);
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
            var msg = ValidateLuceneIndexer(indexerName, out LuceneIndexer indexer);
            if (msg.IsSuccessStatusCode)
            {
                var cacheKey = "temp_indexing_op_" + indexerName;
                var found = ApplicationCache.RuntimeCache.GetCacheItem(cacheKey);

                //if its still there then it's not done
                return found != null
                           ? null
                           : CreateModel(new KeyValuePair<string, IIndexer>(indexerName, indexer));
            }

            throw new HttpResponseException(msg);
        }

        /// <summary>
        ///     Rebuilds the index
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        public HttpResponseMessage PostRebuildIndex(string indexerName)
        {
            var msg = ValidateLuceneIndexer(indexerName, out LuceneIndexer indexer);
            if (msg.IsSuccessStatusCode)
            {
                _logger.Info<ExamineManagementController>("Rebuilding index '{IndexerName}'", indexerName);

                //remove it in case there's a handler there alraedy
                indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;

                //now add a single handler
                indexer.IndexOperationComplete += Indexer_IndexOperationComplete;

                var cacheKey = "temp_indexing_op_" + indexer.Name;

                //put temp val in cache which is used as a rudimentary way to know when the indexing is done
                ApplicationCache.RuntimeCache.InsertCacheItem(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5),
                                                              false);

                try
                {
                    indexer.RebuildIndex();
                }
                catch (Exception ex)
                {
                    //ensure it's not listening
                    indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
                    Logger.Error<ExamineManagementController>(ex, "An error occurred rebuilding index");
                    var response = Request.CreateResponse(HttpStatusCode.Conflict);
                    response.Content =
                        new
                            StringContent($"The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {ex}");
                    response.ReasonPhrase = "Could Not Rebuild";
                    return response;
                }
            }

            return msg;
        }

        private ExamineIndexerModel CreateModel(KeyValuePair<string, IIndexer> indexerKeyVal)
        {
            var indexer = indexerKeyVal.Value;
            var indexName = indexerKeyVal.Key;
            var indexerModel = new ExamineIndexerModel
            {
                FieldDefinitions = indexer.FieldDefinitionCollection,
                Name = indexName
            };

            var props = TypeHelper.CachedDiscoverableProperties(indexer.GetType(), mustWrite: false)
                                  //ignore these properties
                                  .Where(x => new[] { "IndexerData", "Description", "WorkingFolder" }
                                                  .InvariantContains(x.Name) == false)
                                  .OrderBy(x => x.Name);

            foreach (var p in props)
            {
                var val = p.GetValue(indexer, null);
                if (val == null)
                {
                    // Do not warn for new new attribute that is optional
                    if (string.Equals(p.Name, "DirectoryFactory", StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        Logger
                            .Warn<ExamineManagementController
                            >("Property value was null when setting up property on indexer: " + indexName +
                              " property: " + p.Name);
                    }

                    val = string.Empty;
                }

                indexerModel.ProviderProperties.Add(p.Name, val.ToString());
            }

            if (indexer is LuceneIndexer luceneIndexer)
            {
                indexerModel.IsLuceneIndex = true;

                if (luceneIndexer.IndexExists())
                {
                    indexerModel.IsHealthy = luceneIndexer.IsHealthy(out var indexError);

                    if (indexerModel.IsHealthy == false)
                    {
                        //we cannot continue at this point
                        indexerModel.Error = indexError.ToString();
                        return indexerModel;
                    }

                    indexerModel.DocumentCount = luceneIndexer.GetIndexDocumentCount();
                    indexerModel.FieldCount = luceneIndexer.GetIndexFieldCount();
                }
                else
                {
                    indexerModel.DocumentCount = 0;
                    indexerModel.FieldCount = 0;
                }
            }

            return indexerModel;
        }

        private HttpResponseMessage ValidateLuceneSearcher(string searcherName, out LuceneSearcher searcher)
        {
            foreach (var indexer in _examineManager.IndexProviders)
            {
                var s = indexer.Value.GetSearcher();
                var sName = (s as BaseLuceneSearcher)?.Name ?? string.Concat(indexer.Key, "Searcher");
                if (sName != searcherName)
                {
                    continue;
                }

                searcher = s as LuceneSearcher;

                //Found it, return OK
                if (searcher != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                //Return an error since it's not the right type
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content =
                    new StringContent($"The searcher {searcherName} is not of type {typeof(LuceneSearcher)}");
                response.ReasonPhrase = "Wrong Searcher Type";
                return response;
            }

            searcher = null;

            var response1 = Request.CreateResponse(HttpStatusCode.BadRequest);
            response1.Content = new StringContent($"No searcher found with name = {searcherName}");
            response1.ReasonPhrase = "Searcher Not Found";
            return response1;
        }

        private HttpResponseMessage ValidateLuceneIndexer<T>(string indexerName, out T indexer)
            where T : class, IIndexer
        {
            indexer = null;

            if (_examineManager.IndexProviders.ContainsKey(indexerName)
                && _examineManager.IndexProviders[indexerName] is T casted)
            {
                //return Ok!
                indexer = casted;
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            var response = Request.CreateResponse(HttpStatusCode.BadRequest);
            response.Content = new StringContent($"No indexer found with name = {indexerName} of type {typeof(T)}");
            response.ReasonPhrase = "Indexer Not Found";
            return response;
        }

        //static listener so it's not GC'd
        private void Indexer_IndexOperationComplete(object sender, EventArgs e)
        {
            var indexer = (LuceneIndexer) sender;

            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;

            _logger
                .Info<ExamineManagementController
                >($"Rebuilding index '{indexer.Name}' done, {indexer.CommitCount} items committed (can differ from the number of items in the index)");

            var cacheKey = "temp_indexing_op_" + indexer.Name;
            _runtimeCacheProvider.ClearCacheItem(cacheKey);
        }
    }
}
