using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase" />.
/// </summary>
/// <remarks>
///     A pure 1:1 copy of <see cref="IContentServiceBase" /> — same members, unchanged, only the interface name
///     gets the "Async" suffix. This is scaffolding: giving the async hierarchy the exact shape of the sync one
///     up front means later increments only ever swap an implementation, never touch a contract, so migrating one
///     member at a time never risks breaking other consumers of the shared base class (e.g. <see cref="ElementService" />,
///     which still derives from the original synchronous class).
/// </remarks>
public interface IAsyncContentServiceBase : IService
{
    /// <summary>
    ///     Checks the data integrity of the content tree and optionally fixes issues.
    /// </summary>
    /// <param name="options">The options for the data integrity check.</param>
    /// <returns>A <see cref="ContentDataIntegrityReport"/> containing the results of the integrity check.</returns>
    ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
}
