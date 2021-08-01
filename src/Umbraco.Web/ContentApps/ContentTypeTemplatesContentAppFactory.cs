using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class ContentTypeTemplatesContentAppFactory : IContentAppFactory
    {
        private const int Weight = -140;

        private ContentApp _contentTypeApp;

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IContentType _:
                    return _contentTypeApp ?? (_contentTypeApp = new ContentApp()
                    {
                        Alias = "templates",
                        Name = "Templates",
                        Icon = "icon-layout",
                        View = "views/documenttypes/views/templates/templates.html",
                        Weight = Weight
                    });
                default:
                    return null;
            }
        }
    }
}
