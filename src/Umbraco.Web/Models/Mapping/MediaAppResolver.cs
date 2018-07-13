using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MediaAppResolver : IValueResolver<IMedia, MediaItemDisplay, IEnumerable<ContentApp>>
    {
        private static readonly ContentApp _contentApp = new ContentApp
        {
            Alias = "content",
            Name = "Content",
            Icon = "icon-document",
            View = "views/media/apps/content/content.html"
        };

        private static readonly ContentApp _infoApp = new ContentApp
        {
            Alias = "info",
            Name = "Info",
            Icon = "icon-info",
            View = "views/media/apps/info/info.html"
        };

        private static readonly ContentApp _listViewApp = new ContentApp
        {
            Alias = "childItems",
            Name = "Child items",
            Icon = "icon-list",
            View = "views/media/apps/listview/listview.html"
        };

        public IEnumerable<ContentApp> Resolve(IMedia source, MediaItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            var apps = new List<ContentApp>();

            if (source.ContentType.IsContainer || source.ContentType.Alias == Umbraco.Core.Constants.Conventions.MediaTypes.Folder)
            {
                apps.Add(_listViewApp);
            }
            else
            {
                apps.Add(_contentApp);
            }

            apps.Add(_infoApp);

            return apps;
        }
    }

}
