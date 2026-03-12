using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class IndexPresentationFactory : IIndexPresentationFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IIndexingRebuilderService _indexingRebuilderService;
    private readonly ILogger<IndexPresentationFactory> _logger;

    public IndexPresentationFactory(
        IIndexDiagnosticsFactory indexDiagnosticsFactory,
        IIndexRebuilder indexRebuilder,
        IIndexingRebuilderService indexingRebuilderService,
        ILogger<IndexPresentationFactory> logger)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _indexingRebuilderService = indexingRebuilderService;
        _logger = logger;
    }

    /// <inheritdoc />
    [Obsolete("Use CreateAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IndexResponseModel Create(IIndex index)
        => CreateAsync(index).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<IndexResponseModel> CreateAsync(IIndex index)
    {
        var isCorrupt = !TryGetIndexMetric(index, idx => idx.Searcher.Name, index.Name, "searcher name", "Could not determine searcher name because of error.", out string searcherName);

        if (await _indexingRebuilderService.IsRebuildingAsync(index.Name))
        {
            return new IndexResponseModel
            {
                Name = index.Name,
                HealthStatus = new HealthStatusResponseModel
                {
                    Status = isCorrupt ? HealthStatus.Corrupt : HealthStatus.Rebuilding,
                },
                SearcherName = searcherName,
                DocumentCount = 0,
                FieldCount = 0,
            };
        }

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealthyAttempt = indexDiag.IsHealthy();

        var properties = new Dictionary<string, object?>();

        foreach (KeyValuePair<string, object?> property in indexDiag.Metadata)
        {
            if (property.Value is null)
            {
                properties[property.Key] = null;
            }
            else
            {
                Type propertyType = property.Value.GetType();
                properties[property.Key] = propertyType.IsClass && !propertyType.IsArray ? property.Value?.ToString() : property.Value;
            }
        }

        if (TryGetIndexMetric(indexDiag, diag => diag.GetDocumentCount(), index.Name, "document count", 0L, out long documentCount) is false)
        {
            isCorrupt = true;
        }

        if (TryGetIndexMetric(indexDiag, diag => diag.GetFieldNames().Count(), index.Name, "field name count", 0, out int fieldNameCount) is false)
        {
            isCorrupt = true;
        }

        var indexerModel = new IndexResponseModel
        {
            Name = index.Name,
            HealthStatus = new HealthStatusResponseModel
            {
                Status = isCorrupt
                    ? HealthStatus.Corrupt
                    : isHealthyAttempt.Success ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                Message = isHealthyAttempt.Result,
            },
            CanRebuild = isCorrupt is false && _indexRebuilder.CanRebuild(index.Name),
            SearcherName = searcherName,
            DocumentCount = documentCount,
            FieldCount = fieldNameCount,
            ProviderProperties = properties,
        };

        return indexerModel;
    }

    private bool TryGetIndexMetric<TSource, TResult>(
        TSource source,
        Func<TSource, TResult> getMetric,
        string indexName,
        string metricName,
        TResult errorDefault,
        out TResult result)
    {
        try
        {
            result = getMetric(source);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured trying to get the {MetricName} of index {IndexName}", metricName, indexName);
            result = errorDefault;
            return false;
        }
    }
}
