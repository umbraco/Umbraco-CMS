using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IContentTypeValidator<TContentType, TPropertyType, TPropertyTypeContainer>
    where TContentType : ContentTypeBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeBase
    where TPropertyTypeContainer : PropertyTypeContainerBase
{
    ContentTypeOperationStatus ValidateCommon(TContentType contentType);
}
