namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides access to reserved field names that cannot be used for custom properties.
/// </summary>
public interface IReservedFieldNamesService
{
    /// <summary>
    ///     Gets the set of reserved field names for document content types.
    /// </summary>
    /// <returns>A set of field names that are reserved for documents.</returns>
    ISet<string> GetDocumentReservedFieldNames();

    /// <summary>
    ///     Gets the set of reserved field names for media content types.
    /// </summary>
    /// <returns>A set of field names that are reserved for media.</returns>
    ISet<string> GetMediaReservedFieldNames();

    /// <summary>
    ///     Gets the set of reserved field names for member content types.
    /// </summary>
    /// <returns>A set of field names that are reserved for members.</returns>
    ISet<string> GetMemberReservedFieldNames();
}
