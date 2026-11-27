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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of media items.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a UDI is not of type <see cref="GuidUdi"/>.</exception>
    public static Task<IEnumerable<IMedia>> GetByIdsAsync(this IMediaService mediaService, IEnumerable<Udi> ids, CancellationToken cancellationToken)
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

        return mediaService.GetByIdsAsync(guids.Select(x => x.Guid), cancellationToken);
    }
}
