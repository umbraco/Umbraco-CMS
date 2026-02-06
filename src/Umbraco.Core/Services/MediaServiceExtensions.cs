// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Media service extension methods
/// </summary>
/// <remarks>
///     Many of these have to do with UDI lookups but we don't need to add these methods to the service interface since a
///     UDI is just a GUID
///     and the services already support GUIDs
/// </remarks>
public static class MediaServiceExtensions
{
    /// <summary>
    ///     Gets a collection of media items by their UDIs.
    /// </summary>
    /// <param name="mediaService">The media service.</param>
    /// <param name="ids">The collection of UDIs to retrieve.</param>
    /// <returns>A collection of media items.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a UDI is not of type <see cref="GuidUdi"/>.</exception>
    public static IEnumerable<IMedia> GetByIds(this IMediaService mediaService, IEnumerable<Udi> ids)
    {
        var guids = new List<GuidUdi>();
        foreach (Udi udi in ids)
        {
            if (udi is not GuidUdi guidUdi)
            {
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) +
                                                    " which is required by media");
            }

            guids.Add(guidUdi);
        }

        return mediaService.GetByIds(guids.Select(x => x.Guid));
    }

    /// <summary>
    ///     Creates a new media item under a parent specified by UDI.
    /// </summary>
    /// <param name="mediaService">The media service.</param>
    /// <param name="name">The name of the new media item.</param>
    /// <param name="parentId">The UDI of the parent media item.</param>
    /// <param name="mediaTypeAlias">The alias of the media type.</param>
    /// <param name="userId">The ID of the user creating the media item.</param>
    /// <returns>The newly created media item.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the UDI is not of type <see cref="GuidUdi"/>.</exception>
    public static IMedia CreateMedia(this IMediaService mediaService, string name, Udi parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parentId is not GuidUdi guidUdi)
        {
            throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) +
                                                " which is required by media");
        }

        IMedia? parent = mediaService.GetById(guidUdi.Guid);
        return mediaService.CreateMedia(name, parent, mediaTypeAlias, userId);
    }
}
