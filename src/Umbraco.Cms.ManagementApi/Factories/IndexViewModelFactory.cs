﻿using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.Search;
using Umbraco.New.Cms.Infrastructure.Services;

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

    public IndexViewModel Create(IIndex index)
    {
        if (_indexingRebuilderService.IsRebuilding(index.Name))
        {
            return new IndexViewModel
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

        var properties = new Dictionary<string, object?>();

        foreach (var p in indexDiag.Metadata)
        {
            if (p.Value is null)
            {
                properties[p.Key] = null;
            }
            else
            {
                var t = p.Value.GetType();
                properties[p.Key] = t.IsClass && !t.IsArray ? p.Value?.ToString() : p.Value;
            }
        }

        var indexerModel = new IndexViewModel
        {
            Name = index.Name,
            HealthStatus = isHealthy.Success ? isHealthy.Result ?? "Healthy" : isHealthy.Result ?? "Unhealthy",
            CanRebuild = _indexRebuilder.CanRebuild(index.Name),
            SearcherName = index.Searcher.Name,
            DocumentCount = indexDiag.GetDocumentCount(),
            FieldCount = indexDiag.GetFieldNames().Count(),
            ProviderProperties = properties,
        };

        return indexerModel;
    }
}
