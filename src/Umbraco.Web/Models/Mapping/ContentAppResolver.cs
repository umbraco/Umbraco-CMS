using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    // injected into ContentMapperProfile,
    // maps ContentApps when mapping IContent to ContentItemDisplay
    internal class ContentAppResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentApp>>
    {
        private readonly ContentAppFactoryCollection _contentAppDefinitions;

        public ContentAppResolver(ContentAppFactoryCollection contentAppDefinitions)
        {
            _contentAppDefinitions = contentAppDefinitions;
        }

        public IEnumerable<ContentApp> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            return _contentAppDefinitions.GetContentAppsFor(source);
        }
    }
}
