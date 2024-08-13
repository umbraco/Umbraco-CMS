using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IElementSwitchValidator
{
    Task<bool> AncestorsAreAlignedAsync(IContentType contentType);

    Task<bool> DescendantsAreAlignedAsync(IContentType contentType);

    Task<bool> ElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType);

    Task<bool> DocumentToElementHasNoContentAsync(IContentTypeBase contentType);
}
