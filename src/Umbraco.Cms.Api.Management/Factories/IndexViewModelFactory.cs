using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.Factories;

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

    public IndexViewModel Create(IIndex index)
    {
        if (_indexingRebuilderService.IsRebuilding(index.Name))
        {
            return new IndexViewModel
            {
                Name = index.Name,
                HealthStatus = HealthStatus.Rebuilding,
                SearcherName = index.Searcher.Name,
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
            Name = index.Name,
            HealthStatus = isHealthy.Success ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            CanRebuild = _indexRebuilder.CanRebuild(index.Name),
            SearcherName = index.Searcher.Name,
            DocumentCount = indexDiag.GetDocumentCount(),
            FieldCount = indexDiag.GetFieldNames().Count(),
            ProviderProperties = properties,
        };

        return indexerModel;
    }
}
