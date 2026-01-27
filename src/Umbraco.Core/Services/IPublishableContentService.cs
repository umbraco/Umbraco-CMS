using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: fully define this interface
public interface IPublishableContentService<TContent> : IContentServiceBase<TContent>
    where TContent : class, IPublishableContentBase
{
    /// <summary>
    ///     Saves content.
    /// </summary>
    /// <param name="content">The content to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    /// <returns>The operation result.</returns>
    OperationResult Save(TContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    ///     Deletes content.
    /// </summary>
    /// <param name="content">The content to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    ///     <para>This method will also delete associated media files, child content and possibly associated domains.</para>
    ///     <para>This method entirely clears the content from the database.</para>
    /// </remarks>
    OperationResult Delete(TContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all content of given types.
    /// </summary>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted content is moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId">The unique identifier of the content to load schedule for.</param>
    /// <returns>The <see cref="ContentScheduleCollection" />.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content to persist the schedule for.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection contentSchedule);

    /// <summary>
    ///     Publishes content
    /// </summary>
    /// <remarks>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    ///     <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// </remarks>
    /// <param name="content">The content to publish.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    PublishResult Publish(TContent content, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Unpublishes content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, unpublishes the content as a whole, but it is possible to specify a culture to be
    ///         unpublished. Depending on whether that culture is mandatory, and other cultures remain published,
    ///         the content as a whole may or may not remain published.
    ///     </para>
    ///     <para>
    ///         If the content type is variant, then culture can be either '*' or an actual culture, but neither null nor
    ///         empty. If the content type is invariant, then culture can be either '*' or null or empty.
    ///     </para>
    /// </remarks>
    PublishResult Unpublish(TContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets all versions of content.
    /// </summary>
    /// <param name="id">The identifier of the content.</param>
    /// <returns>The content versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<TContent> GetVersions(int id);

    /// <summary>
    ///     Gets all versions of content.
    /// </summary>
    /// <param name="id">The identifier of the content.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The number of versions to take.</param>
    /// <returns>The content versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<TContent> GetVersionsSlim(int id, int skip, int take);

    /// <summary>
    ///     Gets top versions of content.
    /// </summary>
    /// <param name="id">The identifier of the content.</param>
    /// <param name="topRows">The number of top versions to get.</param>
    /// <returns>The version identifiers.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<int> GetVersionIds(int id, int topRows);

    /// <summary>
    ///     Gets a version of content.
    /// </summary>
    /// <param name="versionId">The version identifier.</param>
    /// <returns>The content version, or null if not found.</returns>
    TContent? GetVersion(int versionId);

    /// <summary>
    ///     Rolls back the content to a specific version.
    /// </summary>
    /// <param name="id">The id of the content node.</param>
    /// <param name="versionId">The version id to roll back to.</param>
    /// <param name="culture">An optional culture to roll back.</param>
    /// <param name="userId">The identifier of the user who is performing the roll back.</param>
    /// <remarks>
    ///     <para>When no culture is specified, all cultures are rolled back.</para>
    /// </remarks>
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);
}
