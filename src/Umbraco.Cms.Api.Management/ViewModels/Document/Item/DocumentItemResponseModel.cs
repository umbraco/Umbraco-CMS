﻿using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Item;

// TODO: this model should probably contain URLs
public class DocumentItemResponseModel : ItemResponseModelBase
{
    public bool IsTrashed { get; set; }

    public bool IsProtected { get; set; }

    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}
