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
    ///     The additional index fields that are searched on in the back office for member entities.
    /// </summary>
    IEnumerable<string> GetBackOfficeMembersFields();

    /// <summary>
    ///     The additional index fields that are searched on in the back office for media entities.
    /// </summary>
    IEnumerable<string> GetBackOfficeMediaFields();

    /// <summary>
    ///     The additional index fields that are searched on in the back office for document entities.
    /// </summary>
    IEnumerable<string> GetBackOfficeDocumentFields();

    ISet<string> GetBackOfficeFieldsToLoad();

    ISet<string> GetBackOfficeMembersFieldsToLoad();

    ISet<string> GetBackOfficeDocumentFieldsToLoad();

    ISet<string> GetBackOfficeMediaFieldsToLoad();
}
