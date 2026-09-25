using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase" />.
/// </summary>
/// <remarks>
///     The root of the asynchronous content-service hierarchy shared by documents, elements and members.
///     <see cref="IContentService" /> and <see cref="IElementService" /> back it with asynchronous
///     implementations; <see cref="IMemberService" /> implements it by delegating to its synchronous members so that
///     members can share the asynchronous content editing base. <see cref="IMediaService" /> remains on
///     <see cref="IContentServiceBase" /> alone.
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
