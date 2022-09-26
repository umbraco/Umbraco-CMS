using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.ManagementApi.Factories;

public class ExamineIndexViewModelFactory : IExamineIndexViewModelFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ITemporaryIndexingService _temporaryIndexingService;

    public ExamineIndexViewModelFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, ITemporaryIndexingService temporaryIndexingService)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _temporaryIndexingService = temporaryIndexingService;
    }

    public ExamineIndexViewModel Create(IIndex index)
    {
        if (_temporaryIndexingService.Detect(index.Name))
        {
            return new ExamineIndexViewModel
            {
                Name = index.Name,
                HealthStatus = "Rebuilding",
                SearcherName = index.Searcher.Name,
                DocumentCount = 0,
                FieldCount = 0,
            };
        }

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealthy = indexDiag.IsHealthy();

        var indexerModel = new ExamineIndexViewModel
        {
            Name = index.Name,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? "Healthy" : isHealthy.Result ?? "Unhealthy",
            CanRebuild = _indexRebuilder.CanRebuild(index.Name),
            SearcherName = index.Searcher.Name,
            DocumentCount = indexDiag.GetDocumentCount(),
            FieldCount = indexDiag.GetFieldNames().Count(),
        };

        return indexerModel;
    }
}
