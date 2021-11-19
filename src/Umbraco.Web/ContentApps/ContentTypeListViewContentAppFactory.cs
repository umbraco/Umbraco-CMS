using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class ContentTypeListViewContentAppFactory : IContentAppFactory
    {
        private const int Weight = -180;

        private ContentApp _contentTypeApp;

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IContentType _:
                    return _contentTypeApp ?? (_contentTypeApp = new ContentApp()
                    {
                        Alias = "listView",
                        Name = "List view",
                        Icon = "icon-list",
                        View = "views/documenttypes/views/listview/listview.html",
                        Weight = Weight
                    });
                default:
                    return null;
            }
        }
    }
}
