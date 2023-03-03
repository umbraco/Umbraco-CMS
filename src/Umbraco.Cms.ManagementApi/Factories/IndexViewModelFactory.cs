using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.Search;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Indexing;
using Umbraco.Search.Services;

namespace Umbraco.Cms.ManagementApi.Factories;

public class IndexViewModelFactory : IIndexViewModelFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IIndexingRebuilderService _indexingRebuilderService;

    public IndexViewModelFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, IIndexingRebuilderService indexingRebuilderService)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _indexingRebuilderService = indexingRebuilderService;
    }

    public IndexViewModel Create(string index)
    {
        if (_indexingRebuilderService.IsRebuilding(index))
        {
            return new IndexViewModel
            {
                Name = index,
                HealthStatus = "Rebuilding",
                SearcherName = index,
                DocumentCount = 0,
                FieldCount = 0,
            };
        }

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealthy = indexDiag.IsHealthy();

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

        var indexerModel = new IndexViewModel
        {
            Name = index,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? "Healthy" : isHealthy.Result ?? "Unhealthy",
            CanRebuild = _indexRebuilder.CanRebuild(index),
            SearcherName = index,
            DocumentCount = indexDiag.GetDocumentCount(),
            FieldCount = indexDiag.GetFieldNames().Count(),
            ProviderProperties = properties,
        };

        return indexerModel;
    }
}
