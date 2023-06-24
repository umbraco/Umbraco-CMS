
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using Umbraco.Search;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Indexing;
using Umbraco.Search.Models;
using SearchResult = Umbraco.Cms.Core.Models.ContentEditing.SearchResult;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class SearchManagementController : UmbracoAuthorizedJsonController
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ISearchProvider _provider;
    private readonly ILogger<SearchManagementController> _logger;
    private readonly IAppPolicyCache _runtimeCache;

    public SearchManagementController(
        ISearchProvider provider,
        ILogger<SearchManagementController> logger,
        IIndexDiagnosticsFactory indexDiagnosticsFactory,
        AppCaches appCaches,
        IIndexRebuilder indexRebuilder)
    {
        _provider = provider;
        _logger = logger;
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _runtimeCache = appCaches.RuntimeCache;
        _indexRebuilder = indexRebuilder;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SearchIndexModel> GetIndexerDetails()
        => _provider.GetAllIndexes()
            .Select(index => CreateModel(index))
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer"));

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SearcherModel> GetSearcherDetails()
    {
        var model = new List<SearcherModel>(
            _provider.GetAllSearchers().Select(searcher => new SearcherModel { Name = searcher })
                .OrderBy(x =>
                    x.Name?.TrimEnd("Searcher"))); //order by name , but strip the "Searcher" from the end if it exists
        return model;
    }

    public ActionResult<UmbracoSearchResults?> GetSearchResults(string searcherName, string? query, int pageIndex = 0,
        int pageSize = 20)
    {
        query = query?.Trim();

        if (query.IsNullOrWhiteSpace())
        {
            return UmbracoSearchResults.Empty();
        }

        ActionResult msg = ValidateSearcher(searcherName, out IUmbracoSearcher? searcher);
        if (!msg.IsSuccessStatusCode())
        {
            return msg;
        }

        UmbracoSearchResults? results;

        // NativeQuery will work for a single word/phrase too (but depends on the implementation) the lucene one will work.
        try
        {
            results = searcher?
                .NativeQuery(query,pageIndex, pageSize);
        }
        catch (Exception)
        {
            // will occur if the query parser cannot parse this (i.e. starts with a *)
            return UmbracoSearchResults.Empty();
        }

        return results;

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
    public ActionResult<SearchIndexModel?> PostCheckRebuildIndex(string indexName)
    {
        ActionResult validate = ValidateIndex(indexName, out IUmbracoIndex? index);

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
            : CreateModel(indexName!);
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public IActionResult PostRebuildIndex(string indexName)
    {
        ActionResult validate = ValidateIndex(indexName, out IUmbracoIndex? index);
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
            var cacheKey = "temp_indexing_op_" + index?.Name;
            //put temp val in cache which is used as a rudimentary way to know when the indexing is done
            _runtimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));

            _indexRebuilder.RebuildIndex(indexName);

            return new OkResult();
        }
        catch (Exception ex)
        {
            //ensure it's not listening
            index!.IndexOperationComplete -= Indexer_IndexOperationComplete;
            _logger.LogError(ex, "An error occurred rebuilding index");
            var response = new ConflictObjectResult(
                "The index could not be rebuilt at this time, most likely there is another thread currently writing to the index. Error: {ex}");

            HttpContext.SetReasonPhrase("Could Not Rebuild");
            return response;
        }
    }

    private SearchIndexModel CreateModel(string indexName)
    {
        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(indexName);

        Attempt<HealthStatus?> isHealth = indexDiag.IsHealthy();

        var properties = new Dictionary<string, object?>
        {
            ["DocumentCount"] = indexDiag.GetDocumentCount(), ["FieldCount"] = indexDiag.GetFieldNames().Count()
        };

        foreach (KeyValuePair<string, object?> p in indexDiag.Metadata)
        {
            properties[p.Key] = p.Value;
        }

        var indexerModel = new SearchIndexModel
        {
            Name = indexName,
            HealthStatus = isHealth.Success ? Enum.GetName(typeof(HealthStatus),isHealth.Result ?? HealthStatus.Healthy) : Enum.GetName(typeof(HealthStatus),isHealth.Result ?? HealthStatus.Unhealthy),
            ProviderProperties = properties,
            CanRebuild = _indexRebuilder.CanRebuild(indexName)
        };

        return indexerModel;
    }

    private ActionResult ValidateSearcher(string searcherName, out IUmbracoSearcher? searcher)
    {
        searcher = _provider.GetSearcher(searcherName.Replace("Index","Searcher"));
        if (searcher != null)
        {
            return new OkResult();
        }


        var response1 = new BadRequestObjectResult($"No searcher found with name = {searcherName}");
        HttpContext.SetReasonPhrase("Searcher Not Found");
        return response1;
    }

    private ActionResult ValidatePopulator(IUmbracoIndex index)
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

    private ActionResult ValidateIndex(string indexName, out IUmbracoIndex? index)
    {
        index = _provider.GetIndex(indexName);

        if (index != null)
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
        if (sender == null)
        {
            return;
        }
        var indexer = (IUmbracoIndex)sender;

        _logger.LogDebug("Logging operation completed for index {IndexName}", indexer?.Name);


        //ensure it's not listening anymore
        if (indexer?.IndexOperationComplete != null)
        {
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
        }

        _logger.LogInformation($"Rebuilding index '{indexer?.Name}' done.");

        var cacheKey = "temp_indexing_op_" + indexer?.Name;
        _runtimeCache.Clear(cacheKey);
    }
}
