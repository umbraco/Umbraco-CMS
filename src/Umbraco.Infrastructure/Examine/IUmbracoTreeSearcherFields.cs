namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to propagate hardcoded internal Field lists
/// </summary>
public interface IUmbracoTreeSearcherFields
{
    /// <summary>
    ///     The default index fields that are searched on in the back office search for umbraco content entities.
    /// </summary>
    IEnumerable<string> GetBackOfficeFields();

    /// <summary>
    ///     Gets the additional index fields that are searched in the back office for member entities.
    /// </summary>
    /// <returns>An enumerable collection of index field names used for searching member entities in the back office.</returns>
    IEnumerable<string> GetBackOfficeMembersFields();

    /// <summary>
    ///     The additional index fields that are searched on in the back office for media entities.
    /// </summary>
    IEnumerable<string> GetBackOfficeMediaFields();

    /// <summary>
    /// Returns the additional index fields that are searched in the back office for document entities.
    /// </summary>
    /// <returns>An enumerable collection of index field names used for searching document entities in the back office.</returns>
    IEnumerable<string> GetBackOfficeDocumentFields();

    /// <summary>
    /// Returns the set of field names that should be loaded for back office operations.
    /// </summary>
    /// <returns>A set of field names required for back office functionality.</returns>
    ISet<string> GetBackOfficeFieldsToLoad();

    /// <summary>
    /// Returns the set of field names to be loaded for back office member search or display.
    /// </summary>
    /// <returns>A set of field names relevant to back office members.</returns>
    ISet<string> GetBackOfficeMembersFieldsToLoad();

    /// <summary>
    /// Returns the set of document field names that should be loaded when retrieving documents for the back office.
    /// </summary>
    /// <returns>A set of field names to load for back office document retrieval.</returns>
    ISet<string> GetBackOfficeDocumentFieldsToLoad();

    /// <summary>
    /// Gets the set of media fields that should be loaded for back office search or indexing operations.
    /// </summary>
    /// <returns>A set of strings representing the names of media fields to load for back office operations.</returns>
    ISet<string> GetBackOfficeMediaFieldsToLoad();
}
