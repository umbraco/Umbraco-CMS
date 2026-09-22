using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase" />.
/// </summary>
/// <remarks>
///     Started as a pure 1:1 copy of <see cref="IContentServiceBase" /> — same members, only the interface name
///     getting the "Async" suffix. This is scaffolding: giving the async hierarchy the exact shape of the sync one
///     up front means later increments only ever swap an implementation, never touch a contract, so migrating one
///     member at a time never risks breaking other consumers of the shared base class (e.g. <see cref="ElementService" />,
///     which still derives from the original synchronous class). <see cref="CheckDataIntegrityAsync" /> has since
///     received its real async conversion. <see cref="IContentService" /> and <see cref="IElementService" /> back it
///     with genuinely asynchronous implementations; <see cref="IMemberService" /> implements it by delegating to its
///     synchronous members, which it does because members share the asynchronous content editing base with documents
///     and elements. <see cref="IMediaService" /> remains on <see cref="IContentServiceBase" /> alone.
/// </remarks>
public interface IAsyncContentServiceBase : IService
{
    /// <summary>
    ///     Checks the data integrity of the content tree and optionally fixes issues.
    /// </summary>
    /// <param name="options">The options for the data integrity check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ContentDataIntegrityReport"/> containing the results of the integrity check.</returns>
    Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken);
}
