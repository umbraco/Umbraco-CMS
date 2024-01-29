﻿using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class VariantModelBase : IHasCultureAndSegment
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
