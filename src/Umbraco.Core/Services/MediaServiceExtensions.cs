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

    public static IMedia CreateMedia(this IMediaService mediaService, string name, Udi parentId, string mediaTypeAlias, int userId = 0)
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
