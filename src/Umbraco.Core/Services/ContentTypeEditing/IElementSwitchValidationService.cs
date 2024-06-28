using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IElementSwitchValidationService
{
    Task<bool> AncestorsAreNotMisalignedAsync(IContentType contentType);

    Task<bool> DescendantsAreNotMisalignedAsync(IContentType contentType);
    Task<bool> ElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType);
    Task<bool> DocumentToElementHasNoContentAsync(IContentTypeBase contentType);
}
