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
                case IContent _:
                    return null;
                case IMedia _:
                    return null;
                case IMember _:
                    return null;
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
                    throw new NotSupportedException($"Object type {source.GetType()} is not supported here.");
            }
        }
    }
}
