using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Media service extension methods
    /// </summary>
    /// <remarks>
    /// Many of these have to do with UDI lookups but we don't need to add these methods to the service interface since a UDI is just a GUID
    /// and the services already support GUIDs
    /// </remarks>
    public static class MediaServiceExtensions
    {
        public static IMedia CreateMedia(this IMediaService mediaService, string name, Udi parentId, string mediaTypeAlias, int userId = 0)
        {
            var guidUdi = parentId as GuidUdi;
            if (guidUdi == null)
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by media");
            var parent = mediaService.GetById(guidUdi.Guid);
            return mediaService.CreateMedia(name, parent, mediaTypeAlias, userId);
        }
    }
}