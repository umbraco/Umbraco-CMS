using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public abstract class ContentTypeValidator<TContentType, TPropertyType, TPropertyTypeContainer>
    where TContentType : ContentTypeBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeBase
    where TPropertyTypeContainer : PropertyTypeContainerBase
{
    protected ContentTypeOperationStatus ValidateCommon(TContentType contentType) => throw new NotImplementedException();
}
