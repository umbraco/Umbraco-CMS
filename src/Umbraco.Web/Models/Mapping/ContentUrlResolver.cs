using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentUrlResolver : IValueResolver<IContent, ContentItemDisplay, string[]>
    {
        public string[] Resolve(IContent source, ContentItemDisplay destination, string[] destMember, ResolutionContext context)
        {
            var umbracoContext = context.GetUmbracoContext();

            var urls = umbracoContext == null
                ? new[] {"Cannot generate urls without a current Umbraco Context"}
                : source.GetContentUrls(umbracoContext).ToArray();

            return urls;
        }
    }
}