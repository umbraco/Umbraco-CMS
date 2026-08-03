using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

/// <summary>
/// Serves as the base controller for managing media items via the Umbraco CMS Management API.
/// Provides common functionality for derived media item controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
public class MediaItemControllerBase : ManagementApiControllerBase
{
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaItemControllerBase"/> class.
    /// </summary>
    /// <param name="mediaPresentationFactory">Factory responsible for creating media presentation models.</param>
    public MediaItemControllerBase(IMediaPresentationFactory mediaPresentationFactory)
        => _mediaPresentationFactory = mediaPresentationFactory;

    /// <summary>
    /// Maps a collection of media entities to their response models.
    /// </summary>
    /// <param name="entities">The entities to map.</param>
    /// <returns>The mapped media item response models.</returns>
    protected async Task<IEnumerable<MediaItemResponseModel>> MapMediaItemsAsync(IEnumerable<IEntitySlim> entities)
    {
        List<MediaItemResponseModel> mapped = [];
        foreach (IMediaEntitySlim entity in entities.OfType<IMediaEntitySlim>())
        {
            mapped.Add(await _mediaPresentationFactory.CreateItemResponseModelAsync(entity));
        }

        return mapped;
    }
}
