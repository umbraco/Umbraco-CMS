using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal interface IContentValidationServiceBase<in TContentType>
    where TContentType : IContentTypeComposition
{
    Task<Attempt<IList<PropertyValidationError>, ContentEditingOperationStatus>> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        TContentType contentType);
}
