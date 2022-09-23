using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.ManagementApi.Factories;

public class ExamineIndexViewModelFactory : IExamineIndexViewModelFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ITemporaryIndexingService _temporaryIndexingService;

    public ExamineIndexViewModelFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, AppCaches runtimeCache, ITemporaryIndexingService temporaryIndexingService)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _temporaryIndexingService = temporaryIndexingService;
    }

    public ExamineIndexViewModel Create(IIndex index)
    {
        if (_temporaryIndexingService.Detect(index.Name))
        {
            throw new OperationCanceledException("The index is still rebuilding, so could not get it");
        }

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealthy = indexDiag.IsHealthy();

        var properties = new Dictionary<string, object?>
        {
            ["DocumentCount"] = indexDiag.GetDocumentCount(),
            ["FieldCount"] = indexDiag.GetFieldNames().Count(),
        };

        foreach (KeyValuePair<string, object?> p in indexDiag.Metadata)
        {
            properties[p.Key] = p.Value;
        }

        var indexerModel = new ExamineIndexViewModel
        {
            Name = index.Name,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? "Healthy" : isHealthy.Result ?? "Unhealthy",
            ProviderProperties = properties,
            CanRebuild = _indexRebuilder.CanRebuild(index.Name),
            SearcherName = index.Searcher.Name,
        };

        return indexerModel;
    }
}
