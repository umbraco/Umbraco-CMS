using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    public class ContentInfoContentAppFactory : IContentAppFactory
    {
        // see note on ContentApp
        private const int Weight = +100;

        private ContentApp _contentApp;
        private ContentApp _mediaApp;
        private ContentApp _memberApp;

        public ContentApp GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (o)
            {
                case IContent _:
                    return _contentApp ?? (_contentApp = new ContentApp
                    {
                        Alias = "umbInfo",
                        Name = "Info",
                        Icon = "icon-info",
                        View = "views/content/apps/info/info.html",
                        Weight = Weight
                    });

                case IMedia _:
                    return _mediaApp ?? (_mediaApp = new ContentApp
                    {
                        Alias = "umbInfo",
                        Name = "Info",
                        Icon = "icon-info",
                        View = "views/media/apps/info/info.html",
                        Weight = Weight
                    });
                case IMember _:
                    return _memberApp ?? (_memberApp = new ContentApp
                    {
                        Alias = "umbInfo",
                        Name = "Info",
                        Icon = "icon-info",
                        View = "views/member/apps/info/info.html",
                        Weight = Weight
                    });

                default:
                    return null;
            }
        }
    }
}
