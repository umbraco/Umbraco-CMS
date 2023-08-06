﻿namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentTreeItemResponseModel : ContentTreeItemResponseModel
{
    public bool IsProtected { get; set; }

    public bool IsPublished { get; set; }

    public bool IsEdited { get; set; }

    public Guid ContentTypeId { get; set; }

    public IEnumerable<VariantTreeItemViewModel> Variants { get; set; } = Enumerable.Empty<VariantTreeItemViewModel>();

    public string Icon { get; set; } = string.Empty;
}
