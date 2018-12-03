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
using Umbraco.Examine;
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
        private readonly IEnumerable<IIndexPopulator> _populators;

        public ExamineManagementController(IExamineManager examineManager, ILogger logger,
            IRuntimeCacheProvider runtimeCacheProvider,
            IEnumerable<IIndexPopulator> populators)
        {
            _examineManager = examineManager;
            _logger = logger;
            _runtimeCacheProvider = runtimeCacheProvider;
            _populators = populators;
        }

        /// <summary>
        /// Get the details for indexers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineIndexModel> GetIndexerDetails()
        {
            return _examineManager.Indexes.Select(CreateModel).OrderBy(x => x.Name.TrimEnd("Indexer"));
        }

        /// <summary>
        /// Get the details for searchers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineSearcherModel> GetSearcherDetails()
        {
            var model = new List<ExamineSearcherModel>(
                _examineManager.RegisteredSearchers.Select(searcher => new ExamineSearcherModel{Name = searcher.Name})
                    .OrderBy(x => x.Name.TrimEnd("Searcher"))); //order by name , but strip the "Searcher" from the end if it exists
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
        /// <param name="indexName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is kind of rudimentary since there's no way we can know that the index has rebuilt, we
        /// have a listener for the index op complete so we'll just check if that key is no longer there in the runtime cache
        /// </remarks>
        public ExamineIndexModel PostCheckRebuildIndex(string indexName)
        {
            var validate = ValidateIndex(indexName, out var index);
            if (!validate.IsSuccessStatusCode)
                throw new HttpResponseException(validate);

            validate = ValidatePopulator(indexName);
            if (!validate.IsSuccessStatusCode)
                throw new HttpResponseException(validate);

            var cacheKey = "temp_indexing_op_" + indexName;
            var found = ApplicationCache.RuntimeCache.GetCacheItem(cacheKey);

            //if its still there then it's not done
            return found != null
                ? null
                : CreateModel(index);

        }

        /// <summary>
        /// Rebuilds the index
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public HttpResponseMessage PostRebuildIndex(string indexName)
        {
            var validate = ValidateIndex(indexName, out var index);
            if (!validate.IsSuccessStatusCode)
                return validate;

            validate = ValidatePopulator(indexName);
            if (!validate.IsSuccessStatusCode)
                return validate;

            _logger.Info<ExamineManagementController>("Rebuilding index '{IndexName}'", indexName);

            //remove it in case there's a handler there alraedy
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;

            //now add a single handler
            index.IndexOperationComplete += Indexer_IndexOperationComplete;

            var cacheKey = "temp_indexing_op_" + index.Name;

            //put temp val in cache which is used as a rudimentary way to know when the indexing is done
            ApplicationCache.RuntimeCache.InsertCacheItem(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));

            try
            {
                //clear and replace
                index.CreateIndex();

                //populate it
                foreach (var populator in _populators.Where(x => x.IsRegistered(indexName)))
                    populator.Populate(index);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //ensure it's not listening
                index.IndexOperationComplete -= Indexer_IndexOperationComplete;
                Logger.Error<ExamineManagementController>(ex, "An error occurred rebuilding index");
                var response = Request.CreateResponse(HttpStatusCode.Conflict);
                response.Content =
                    new
                        StringContent($"The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {ex}");
                response.ReasonPhrase = "Could Not Rebuild";
                return response;
            }
        }

        

        private ExamineIndexModel CreateModel(IIndex index)
        {
            var indexName = index.Name;

            if (!(index is IIndexDiagnostics indexDiag))
                indexDiag = new GenericIndexDiagnostics(index);


            var isHealth = indexDiag.IsHealthy();
            var properties = new Dictionary<string, object>
            {
                [nameof(IIndexDiagnostics.DocumentCount)] = indexDiag.DocumentCount,
                [nameof(IIndexDiagnostics.FieldCount)] = indexDiag.FieldCount,
            };
            foreach (var p in indexDiag.Metadata)
                properties[p.Key] = p.Value;

            var indexerModel = new ExamineIndexModel
            {
                Name = indexName,
                HealthStatus = isHealth.Success ? (isHealth.Result ?? "Healthy") : (isHealth.Result ?? "Unhealthy"),
                ProviderProperties = properties,
                CanRebuild = _populators.Any(x => x.IsRegistered(indexName))
            };
            

            return indexerModel;
        }

        private HttpResponseMessage ValidateLuceneSearcher(string searcherName, out LuceneSearcher searcher)
        {
            foreach (var indexer in _examineManager.Indexes)
            {
                var s = indexer.GetSearcher();
                var sName = (s as BaseLuceneSearcher)?.Name ?? string.Concat(indexer.Name, "Searcher");
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

        private HttpResponseMessage ValidatePopulator(string indexName)
        {
            if (_populators.Any(x => x.IsRegistered(indexName)))
                return Request.CreateResponse(HttpStatusCode.OK);

            var response = Request.CreateResponse(HttpStatusCode.BadRequest);
            response.Content = new StringContent($"The index {indexName} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}");
            response.ReasonPhrase = "Index cannot be rebuilt";
            return response;
        }

        private HttpResponseMessage ValidateIndex(string indexName, out IIndex index)
        {
            index = null;

            if (_examineManager.TryGetIndex(indexName, out index))
            {
                //return Ok!
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            var response = Request.CreateResponse(HttpStatusCode.BadRequest);
            response.Content = new StringContent($"No index found with name = {indexName}");
            response.ReasonPhrase = "Index Not Found";
            return response;
        }

        //static listener so it's not GC'd
        private void Indexer_IndexOperationComplete(object sender, EventArgs e)
        {
            var indexer = (LuceneIndex) sender;

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
