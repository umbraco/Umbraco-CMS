using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Factories;

public class ExamineIndexViewModelFactory : IExamineIndexViewModelFactory
{
    private readonly IIndexDiagnosticsFactory _indexDiagnosticsFactory;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IAppPolicyCache _runtimeCache;

    public ExamineIndexViewModelFactory(IIndexDiagnosticsFactory indexDiagnosticsFactory, IIndexRebuilder indexRebuilder, AppCaches runtimeCache)
    {
        _indexDiagnosticsFactory = indexDiagnosticsFactory;
        _indexRebuilder = indexRebuilder;
        _runtimeCache = runtimeCache.RuntimeCache;
    }

    public ExamineIndexViewModel Create(IIndex index)
    {
        var cacheKey = "temp_indexing_op_" + index.Name;
        var found = _runtimeCache.Get(cacheKey);
        if (found is not null)
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
        };

        return indexerModel;
    }
}
