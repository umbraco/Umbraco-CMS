using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: make IContentService inherit this interface
public interface IPublishableContentService<TContent>
    where TContent : class, IPublishableContentBase
{
    /// <summary>
    ///     Saves content.
    /// </summary>
    OperationResult Save(TContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    ///     Deletes content.
    /// </summary>
    /// <remarks>
    ///     <para>This method will also delete associated media files, child content and possibly associated domains.</para>
    ///     <para>This method entirely clears the content from the database.</para>
    /// </remarks>
    OperationResult Delete(TContent content, int userId = Constants.Security.SuperUserId);

    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId);
}
