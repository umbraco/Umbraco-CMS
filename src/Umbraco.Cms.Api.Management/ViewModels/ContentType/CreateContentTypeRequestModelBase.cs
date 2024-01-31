﻿namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public Guid? Id { get; set; }

    public ReferenceByIdModel? Folder { get; set; }
}
