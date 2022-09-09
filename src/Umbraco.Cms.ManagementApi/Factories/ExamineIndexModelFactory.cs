using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Factories;

public class ExamineIndexModelFactory : IExamineIndexModelFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;

    public ExamineIndexModelFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
    }

    public ExamineIndexViewModel Create(IIndex index)
    {
        var indexName = index.Name;

        IIndexDiagnostics indexDiag = _indexDiagnosticsFactory.Create(index);

        Attempt<string?> isHealthy = indexDiag.IsHealthy();

        var properties = new Dictionary<string, object?>
        {
            ["DocumentCount"] = indexDiag.GetDocumentCount(),
            ["FieldCount"] = indexDiag.GetFieldNames().Count()
        };

        foreach (KeyValuePair<string, object?> p in indexDiag.Metadata)
        {
            properties[p.Key] = p.Value;
        }

        var indexerModel = new ExamineIndexViewModel
        {
            Name = indexName,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? "Healthy" : isHealthy.Result ?? "Unhealthy",
            ProviderProperties = properties,
            CanRebuild = _indexRebuilder.CanRebuild(index.Name),
        };

        return indexerModel;
    }
}
