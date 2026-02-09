using Umbraco.Cms.Core.Models.ContentQuery;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Querying;

/// <summary>
/// Defines the ContentQueryService, which is an easy access to operations involving <see cref="ContentScheduleQueryResult"/>
/// </summary>
public interface IContentQueryService
{
    /// <summary>
    /// Gets the content with schedules.
    /// </summary>
    /// <param name="id">The id of the content.</param>
    /// <returns>The content with schedules.</returns>
    Task<Attempt<ContentScheduleQueryResult?, ContentQueryOperationStatus>> GetWithSchedulesAsync(Guid id);
}
