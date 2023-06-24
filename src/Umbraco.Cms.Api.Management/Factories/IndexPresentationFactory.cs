using Umbraco.Cms.Core;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Indexing;
using Umbraco.Search.Models;
using Umbraco.Search.Services;
using Umbraco.Search.Services.Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class IndexPresentationFactory : IIndexPresentationFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IIndexingRebuilderService _indexingRebuilderService;

    public IndexPresentationFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, IIndexingRebuilderService indexingRebuilderService)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _indexingRebuilderService = indexingRebuilderService;
    }

    public IndexResponseModel Create(string index)
    {
        if (_indexingRebuilderService.IsRebuilding(index))
        {
            return new IndexResponseModel
            {
                Name = index,
                HealthStatus = HealthStatus.Rebuilding,
                SearcherName = index,
                DocumentCount = 0,
                FieldCount = 0,
            };
        }

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<HealthStatus?> isHealthy = indexDiag.IsHealthy();

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

        var indexerModel = new IndexResponseModel
        {
            Name = index,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? HealthStatus.Healthy : isHealthy.Result ?? HealthStatus.Unhealthy,
            CanRebuild = _indexRebuilder.CanRebuild(index),
            SearcherName = index,
            DocumentCount = indexDiag.GetDocumentCount(),
            FieldCount = indexDiag.GetFieldNames().Count(),
            ProviderProperties = properties,
        };

        return indexerModel;
    }
}
