using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class ContentTypeDesignContentAppFactory : IContentAppFactory
    {
        private const int Weight = -200;

        private ContentApp _contentTypeApp;

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IContentType _:
                    return _contentTypeApp ?? (_contentTypeApp = new ContentApp()
                    {
                        Alias = "design",
                        Name = "Design",
                        Icon = "icon-document-dashed-line",
                        View = "views/documenttypes/views/design/design.html",
                        Weight = Weight
                    });
                default:
                    return null;
            }
        }
    }
}
