using System;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    public static class ServiceContextExtensions
    {
        public static IContentTypeServiceBase<T> GetContentTypeService<T>(this ServiceContext services)
            where T : IContentTypeComposition
        {
            if (typeof(T).Implements<IContentType>())
                return services.ContentTypeService as IContentTypeServiceBase<T>;
            if (typeof(T).Implements<IMediaType>())
                return services.MediaTypeService as IContentTypeServiceBase<T>;
            if (typeof(T).Implements<IMemberType>())
                return services.MemberTypeService as IContentTypeServiceBase<T>;
            throw new ArgumentException("Type " + typeof(T).FullName + " does not have a service.");
        }
    }
}
