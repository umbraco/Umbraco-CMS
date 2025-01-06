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

    [Obsolete("Use the non obsolete method instead. Scheduled for removal in v17")]
    public IndexPresentationFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, IIndexingRebuilderService indexingRebuilderService)
    :this(
        indexDiagnosticsFactory,
        indexRebuilder,
        indexingRebuilderService,
        StaticServiceProvider.Instance.GetRequiredService<ILogger<IndexPresentationFactory>>())
    {
    }

    public IndexResponseModel Create(IIndex index)
    {
        var isCorrupt = !TryGetSearcherName(index, out var searcherName);

        if (_indexingRebuilderService.IsRebuilding(index.Name))
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

        foreach (var property in indexDiag.Metadata)
        {
            if (property.Value is null)
            {
                properties[property.Key] = null;
            }
            else
            {
                var propertyType = property.Value.GetType();
                properties[property.Key] = propertyType.IsClass && !propertyType.IsArray ? property.Value?.ToString() : property.Value;
            }
        }

        if (TryGetDocumentCount(indexDiag, index, out var documentCount) is false)
        {
            isCorrupt = true;
        }

        if (TryGetFieldNameCount(indexDiag, index, out var fieldNameCount) is false)
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

    private bool TryGetSearcherName(IIndex index, out string name)
    {
        try
        {
            name = index.Searcher.Name;
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured trying to get the searcher name of index {IndexName}", index.Name);
            name = "Could not determine searcher name because of error.";
            return false;
        }
    }

    private bool TryGetDocumentCount(IIndexDiagnostics indexDiag, IIndex index, out long documentCount)
    {
        try
        {
            documentCount = indexDiag.GetDocumentCount();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured trying to get the document count of index {IndexName}", index.Name);
            documentCount = 0;
            return false;
        }
    }

    private bool TryGetFieldNameCount(IIndexDiagnostics indexDiag, IIndex index, out int fieldNameCount)
    {
        try
        {
            fieldNameCount = indexDiag.GetFieldNames().Count();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured trying to get the field name count of index {IndexName}", index.Name);
            fieldNameCount = 0;
            return false;
        }
    }
}
