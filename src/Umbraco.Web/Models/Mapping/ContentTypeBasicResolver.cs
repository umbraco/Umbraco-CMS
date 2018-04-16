using System;
using System.Linq;
using System.Web;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Resolves a <see cref="ContentTypeBasic"/> from the <see cref="IContent"/> item and checks if the current user
    /// has access to see this data
    /// </summary>
    internal class ContentTypeBasicResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, ContentTypeBasic>
        where TSource : IContentBase
    {
        public ContentTypeBasic Resolve(TSource source, TDestination destination, ContentTypeBasic destMember, ResolutionContext context)
        {
            //TODO: We can resolve the UmbracoContext from the IValueResolver options!
            // OMG
            if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
            {
                ContentTypeBasic contentTypeBasic;
                if (source is IContent content)
                    contentTypeBasic = Mapper.Map<IContentType, ContentTypeBasic>(content.ContentType);
                else if (source is IMedia media)
                    contentTypeBasic = Mapper.Map<IMediaType, ContentTypeBasic>(media.ContentType);
                else
                    throw new NotSupportedException($"Expected TSource to be IContent or IMedia, got {typeof(TSource).Name}.");

                return contentTypeBasic;
            }
            //no access
            return null;
        }
    }
}
