﻿namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeCreateModel : ContentTypeModelBase, IContentTypeCreate
{
    public Guid? Key { get; set; }

    public Guid? ParentKey { get; set; }
}
