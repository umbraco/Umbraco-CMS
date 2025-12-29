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
}
