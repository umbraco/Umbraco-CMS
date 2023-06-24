﻿using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Indexer;
[Obsolete("This is obsolete and will be removed in future versions of Umbraco, please use Umbraco.Search.Models.IndexResponseModel instead")]
public class IndexResponseModel
{
    [Required]
    public string Name { get; init; } = null!;

    public HealthStatus HealthStatus { get; init; }

    [Required]
    public bool CanRebuild { get; init; }

    public string SearcherName { get; init; } = null!;

    [Required]
    public long DocumentCount { get; init; }

    [Required]
    public int FieldCount { get; init; }

    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; init; }
}
