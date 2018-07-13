using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{

    internal class ContentAppResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentApp>>
    {
        private readonly ContentApp _contentApp = new ContentApp
        {
            Alias = "content",
            Name = "Content",
            Icon = "icon-document",
            View = "views/content/apps/content/content.html"
        };

        private readonly ContentApp _infoApp = new ContentApp
        {
            Alias = "info",
            Name = "Info",
            Icon = "icon-info",
            View = "views/content/apps/info/info.html"
        };

        private static readonly ContentApp _listViewApp = new ContentApp
        {
            Alias = "childItems",
            Name = "Child items",
            Icon = "icon-list",
            View = "views/content/apps/listview/listview.html"
        };

        public IEnumerable<ContentApp> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            var apps = new List<ContentApp>
            {
                 _contentApp,
                _infoApp
            };

            if (source.ContentType.IsContainer)
            {
                apps.Add(_listViewApp);
            }

            return apps;
        }
    }
    
}
