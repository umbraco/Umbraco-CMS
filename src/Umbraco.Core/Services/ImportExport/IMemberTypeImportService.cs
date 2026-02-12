using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service for importing member types from XML files.
/// </summary>
public interface IMemberTypeImportService
{
    /// <summary>
    ///     Imports a member type from a temporary file containing XML data.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file containing the member type XML definition.</param>
    /// <param name="userKey">The unique key of the user performing the import operation.</param>
    /// <param name="mediaTypeId">
    ///     Optional. The unique identifier of an existing member type to update.
    ///     When <c>null</c>, a new member type will be created.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the imported <see cref="IMemberType"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="MemberTypeImportOperationStatus"/> on failure.
    /// </returns>
    Task<Attempt<IMemberType?, MemberTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? mediaTypeId = null);
}
