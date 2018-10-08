using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    // injected into ContentMapperProfile,
    // maps ContentApps when mapping IMedia to MediaItemDisplay
    internal class MediaAppResolver : IValueResolver<IMedia, MediaItemDisplay, IEnumerable<ContentApp>>
    {
        private readonly ContentAppDefinitionCollection _contentAppDefinitions;

        public MediaAppResolver(ContentAppDefinitionCollection contentAppDefinitions)
        {
            _contentAppDefinitions = contentAppDefinitions;
        }

        public IEnumerable<ContentApp> Resolve(IMedia source, MediaItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            return _contentAppDefinitions.GetContentAppsFor(source);
        }
    }
}
