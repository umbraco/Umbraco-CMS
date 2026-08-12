using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ElementService, which is an easy access to operations involving <see cref="IElement" />
/// </summary>
public interface IElementService : IPublishableContentService<IElement>, IAsyncPublishableContentService<IElement>
{
    // IPublishableContentService<IElement> and IAsyncPublishableContentService<IElement> both declare these
    // members with an identical signature (Save directly, CheckDataIntegrity via IContentServiceBase and
    // IAsyncContentServiceBase respectively), so without redeclaring them here every call site that holds an
    // IElementService reference and invokes them directly is ambiguous (CS0121). Save is redeclared returning
    // the plain OperationResult (mirroring IContentService's own Save redeclaration) so it's satisfied
    // implicitly by ElementService's existing plain Save method - no explicit shim needed.
    new OperationResult Save(IEnumerable<IElement> contents, int userId = Constants.Security.SuperUserId);

    new ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
}
