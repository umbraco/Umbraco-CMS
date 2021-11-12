using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Search;
using SearchResult = Umbraco.Web.Models.ContentEditing.SearchResult;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class ExamineManagementController : UmbracoAuthorizedJsonController
    {
        private readonly IExamineManager _examineManager;
        private readonly ILogger _logger;
        private readonly IAppPolicyCache _runtimeCache;
        private readonly IndexRebuilder _indexRebuilder;

        public ExamineManagementController(IExamineManager examineManager, ILogger logger,
            AppCaches appCaches,
            IndexRebuilder indexRebuilder)
        {
            _examineManager = examineManager;
            _logger = logger;
            _runtimeCache = appCaches.RuntimeCache;
            _indexRebuilder = indexRebuilder;
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
                _examineManager.RegisteredSearchers.Select(searcher => new ExamineSearcherModel { Name = searcher.Name })
                    .OrderBy(x => x.Name.TrimEnd("Searcher"))); //order by name , but strip the "Searcher" from the end if it exists
            return model;
        }

        public SearchResults GetSearchResults(string searcherName, string query, int pageIndex = 0, int pageSize = 20)
        {
            if (query.IsNullOrWhiteSpace())
                return SearchResults.Empty();

            var msg = ValidateSearcher(searcherName, out var searcher);
            if (!msg.IsSuccessStatusCode)
                throw new HttpResponseException(msg);

            var results = Examine.ExamineExtensions.TryParseLuceneQuery(query)
                ? searcher.CreateQuery().NativeQuery(query).Execute(maxResults: pageSize * (pageIndex + 1))
                : searcher.Search(query, maxResults: pageSize * (pageIndex + 1));

            var pagedResults = results.Skip(pageIndex * pageSize);

            return new SearchResults
            {
                TotalRecords = results.TotalItemCount,
                Results = pagedResults.Select(x => new SearchResult
                {
                    Id = x.Id,
                    Score = x.Score,
                    Values = x.AllValues.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value)
                })
            };
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

            validate = ValidatePopulator(index);
            if (!validate.IsSuccessStatusCode)
                throw new HttpResponseException(validate);

            var cacheKey = "temp_indexing_op_" + indexName;
            var found = AppCaches.RuntimeCache.Get(cacheKey);

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

            validate = ValidatePopulator(index);
            if (!validate.IsSuccessStatusCode)
                return validate;

            _logger.Info<ExamineManagementController, string>("Rebuilding index '{IndexName}'", indexName);

            //remove it in case there's a handler there already
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;

            //now add a single handler
            index.IndexOperationComplete += Indexer_IndexOperationComplete;

            try
            {
                var cacheKey = "temp_indexing_op_" + index.Name;
                //put temp val in cache which is used as a rudimentary way to know when the indexing is done
                AppCaches.RuntimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));

                _indexRebuilder.RebuildIndex(indexName);

                ////populate it
                //foreach (var populator in _populators.Where(x => x.IsRegistered(indexName)))
                //    populator.Populate(index);

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
            {
                if (index is LuceneIndex luceneIndex)
                    indexDiag = new LuceneIndexDiagnostics(luceneIndex, Logger);
                else
                    indexDiag = new GenericIndexDiagnostics(index);
            }

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
                CanRebuild = _indexRebuilder.CanRebuild(index)
            };

            return indexerModel;
        }

        private HttpResponseMessage ValidateSearcher(string searcherName, out ISearcher searcher)
        {
            //try to get the searcher from the indexes
            if (_examineManager.TryGetIndex(searcherName, out var index))
            {
                searcher = index.GetSearcher();
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            //if we didn't find anything try to find it by an explicitly declared searcher
            if (_examineManager.TryGetSearcher(searcherName, out searcher))
                return Request.CreateResponse(HttpStatusCode.OK);

            var response1 = Request.CreateResponse(HttpStatusCode.BadRequest);
            response1.Content = new StringContent($"No searcher found with name = {searcherName}");
            response1.ReasonPhrase = "Searcher Not Found";
            return response1;
        }

        private HttpResponseMessage ValidatePopulator(IIndex index)
        {
            if (_indexRebuilder.CanRebuild(index))
                return Request.CreateResponse(HttpStatusCode.OK);

            var response = Request.CreateResponse(HttpStatusCode.BadRequest);
            response.Content = new StringContent($"The index {index.Name} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}");
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

        private void Indexer_IndexOperationComplete(object sender, EventArgs e)
        {
            var indexer = (IIndex)sender;

            _logger.Debug<ExamineManagementController, string>("Logging operation completed for index {IndexName}", indexer.Name);

            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;

            _logger
                .Info<ExamineManagementController
                >($"Rebuilding index '{indexer.Name}' done.");

            var cacheKey = "temp_indexing_op_" + indexer.Name;
            _runtimeCache.Clear(cacheKey);
        }
    }
}
