﻿using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentEditingFactory
{
    ContentCreateModel MapCreateModel(DocumentCreateRequestModel createRequestModel);

    ContentUpdateModel MapUpdateModel(DocumentUpdateRequestModel updateRequestModel);
}
