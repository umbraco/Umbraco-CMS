using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Services;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    // injected into ContentMapperProfile,
    // maps ContentApps when mapping IContent to ContentItemDisplay
    internal class ContentAppResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentApp>>
    {
        private readonly ContentAppFactoryCollection _contentAppDefinitions;
        private readonly ILocalizedTextService _localizedTextService;

        public ContentAppResolver(ContentAppFactoryCollection contentAppDefinitions, ILocalizedTextService localizedTextService)
        {
            _contentAppDefinitions = contentAppDefinitions;
            _localizedTextService = localizedTextService;
        }

        public IEnumerable<ContentApp> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            var apps = _contentAppDefinitions.GetContentAppsFor(source).ToArray();

            // localize content app names
            foreach (var app in apps)
            {
                var localizedAppName = _localizedTextService.Localize($"apps/{app.Alias}");
                if (localizedAppName.Equals($"[{app.Alias}]", StringComparison.OrdinalIgnoreCase) == false)
                {
                    app.Name = localizedAppName;
                }
            }

            return apps;
        }
    }
}
