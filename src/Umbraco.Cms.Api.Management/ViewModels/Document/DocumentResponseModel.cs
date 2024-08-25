﻿namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : DocumentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public IEnumerable<DocumentUrlInfo> Urls { get; set; } = Enumerable.Empty<DocumentUrlInfo>();

    public ReferenceByIdModel? Template { get; set; }

    public bool IsTrashed { get; set; }
}
