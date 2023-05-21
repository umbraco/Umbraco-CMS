using Examine;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using SearchResult = Umbraco.Cms.Core.Models.ContentEditing.SearchResult;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class ExamineManagementController : UmbracoAuthorizedJsonController
{
    private readonly IExamineManager _examineManager;
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<ExamineManagementController> _logger;
    private readonly IAppPolicyCache _runtimeCache;

    public ExamineManagementController(
        IExamineManager examineManager,
        ILogger<ExamineManagementController> logger,
        IIndexDiagnosticsFactory indexDiagnosticsFactory,
        AppCaches appCaches,
        IIndexRebuilder indexRebuilder)
    {
        _examineManager = examineManager;
        _logger = logger;
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _runtimeCache = appCaches.RuntimeCache;
        _indexRebuilder = indexRebuilder;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ExamineIndexModel> GetIndexerDetails()
        => _examineManager.Indexes
            .Select(index => CreateModel(index))
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer"));

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ExamineSearcherModel> GetSearcherDetails()
    {
        var model = new List<ExamineSearcherModel>(
            _examineManager.RegisteredSearchers.Select(searcher => new ExamineSearcherModel { Name = searcher.Name })
                .OrderBy(x =>
                    x.Name?.TrimEnd("Searcher"))); //order by name , but strip the "Searcher" from the end if it exists
        return model;
    }

    public ActionResult<SearchResults> GetSearchResults(string searcherName, string? query, int pageIndex = 0, int pageSize = 20)
    {
        query = query?.Trim();

        if (query.IsNullOrWhiteSpace())
        {
            return SearchResults.Empty();
        }

        ActionResult msg = ValidateSearcher(searcherName, out ISearcher searcher);
        if (!msg.IsSuccessStatusCode())
        {
            return msg;
        }

        ISearchResults results;

        // NativeQuery will work for a single word/phrase too (but depends on the implementation) the lucene one will work.
        try
        {
            results = searcher
                .CreateQuery()
                .NativeQuery(query)
                .Execute(QueryOptions.SkipTake(pageSize * pageIndex, pageSize));
        }
        catch (ParseException)
        {
            // will occur if the query parser cannot parse this (i.e. starts with a *)
            return SearchResults.Empty();
        }

        return new SearchResults
        {
            TotalRecords = results.TotalItemCount,
            Results = results.Select(x => new SearchResult
            {
                Id = x.Id,
                Score = x.Score,
                Values = x.AllValues.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value)
            })
        };
    }

    /// <summary>
    ///     Check if the index has been rebuilt
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is kind of rudimentary since there's no way we can know that the index has rebuilt, we
    ///     have a listener for the index op complete so we'll just check if that key is no longer there in the runtime cache
    /// </remarks>
    public ActionResult<ExamineIndexModel?> PostCheckRebuildIndex(string indexName)
    {
        ActionResult validate = ValidateIndex(indexName, out IIndex? index);

        if (!validate.IsSuccessStatusCode())
        {
            return validate;
        }

        validate = ValidatePopulator(index!);
        if (!validate.IsSuccessStatusCode())
        {
            return validate;
        }

        var cacheKey = "temp_indexing_op_" + indexName;
        var found = _runtimeCache.Get(cacheKey);

        //if its still there then it's not done
        return found != null
            ? null
            : CreateModel(index!);
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public IActionResult PostRebuildIndex(string indexName)
    {
        ActionResult validate = ValidateIndex(indexName, out IIndex? index);
        if (!validate.IsSuccessStatusCode())
        {
            return validate;
        }

        validate = ValidatePopulator(index!);
        if (!validate.IsSuccessStatusCode())
        {
            return validate;
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        //remove it in case there's a handler there already
        index!.IndexOperationComplete -= Indexer_IndexOperationComplete;

        //now add a single handler
        index.IndexOperationComplete += Indexer_IndexOperationComplete;

        try
        {
            var cacheKey = "temp_indexing_op_" + index.Name;
            //put temp val in cache which is used as a rudimentary way to know when the indexing is done
            _runtimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));

            _indexRebuilder.RebuildIndex(indexName);

            return new OkResult();
        }
        catch (Exception ex)
        {
            //ensure it's not listening
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;
            _logger.LogError(ex, "An error occurred rebuilding index");
            var response = new ConflictObjectResult(
                "The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {ex}");

            HttpContext.SetReasonPhrase("Could Not Rebuild");
            return response;
        }
    }

    private ExamineIndexModel CreateModel(IIndex index)
    {
        var indexName = index.Name;

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealth = indexDiag.IsHealthy();

        var properties = new Dictionary<string, object?>
        {
            ["DocumentCount"] = indexDiag.GetDocumentCount(),
            ["FieldCount"] = indexDiag.GetFieldNames().Count()
        };

        foreach (KeyValuePair<string, object?> p in indexDiag.Metadata)
        {
            properties[p.Key] = p.Value;
        }

        var indexerModel = new ExamineIndexModel
        {
            Name = indexName,
            HealthStatus = isHealth.Success ? isHealth.Result ?? "Healthy" : isHealth.Result ?? "Unhealthy",
            ProviderProperties = properties,
            CanRebuild = _indexRebuilder.CanRebuild(index.Name)
        };

        return indexerModel;
    }

    private ActionResult ValidateSearcher(string searcherName, out ISearcher searcher)
    {
        //try to get the searcher from the indexes
        if (_examineManager.TryGetIndex(searcherName, out IIndex index))
        {
            searcher = index.Searcher;
            return new OkResult();
        }

        //if we didn't find anything try to find it by an explicitly declared searcher
        if (_examineManager.TryGetSearcher(searcherName, out searcher))
        {
            return new OkResult();
        }

        var response1 = new BadRequestObjectResult($"No searcher found with name = {searcherName}");
        HttpContext.SetReasonPhrase("Searcher Not Found");
        return response1;
    }

    private ActionResult ValidatePopulator(IIndex index)
    {
        if (_indexRebuilder.CanRebuild(index.Name))
        {
            return new OkResult();
        }

        var response = new BadRequestObjectResult(
            $"The index {index.Name} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}");
        HttpContext.SetReasonPhrase("Index cannot be rebuilt");
        return response;
    }

    private ActionResult ValidateIndex(string indexName, out IIndex? index)
    {
        index = null;

        if (_examineManager.TryGetIndex(indexName, out index))
        {
            //return Ok!
            return new OkResult();
        }

        var response = new BadRequestObjectResult($"No index found with name = {indexName}");
        HttpContext.SetReasonPhrase("Index Not Found");
        return response;
    }

    private void Indexer_IndexOperationComplete(object? sender, EventArgs e)
    {
        var indexer = (IIndex?)sender;

        _logger.LogDebug("Logging operation completed for index {IndexName}", indexer?.Name);

        if (indexer is not null)
        {
            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
        }

        _logger.LogInformation($"Rebuilding index '{indexer?.Name}' done.");

        var cacheKey = "temp_indexing_op_" + indexer?.Name;
        _runtimeCache.Clear(cacheKey);
    }
}
