﻿using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public abstract class RecycleBinItemResponseModelBase
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public bool HasChildren { get; set; }

    public ItemReferenceByIdResponseModel? Parent { get; set; }
}
