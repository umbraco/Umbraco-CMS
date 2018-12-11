using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class ContentEditorContentAppFactory : IContentAppFactory
    {
        // see note on ContentApp
        private const int Weight = -100;

        private ContentApp _contentApp;
        private ContentApp _mediaApp;

        public ContentApp GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (o)
            {
                case IContent _:
                    return _contentApp ?? (_contentApp = new ContentApp
                    {
                        Alias = "umbContent",
                        Name = "Content",
                        Icon = "icon-document",
                        View = "views/content/apps/content/content.html",
                        Weight = Weight
                    });

                case IMedia media when !media.ContentType.IsContainer && media.ContentType.Alias != Core.Constants.Conventions.MediaTypes.Folder:
                    return _mediaApp ?? (_mediaApp = new ContentApp
                    {
                        Alias = "umbContent",
                        Name = "Content",
                        Icon = "icon-document",
                        View = "views/media/apps/content/content.html",
                        Weight = Weight
                    });

                case IMedia _:
                    return null;

                default:
                    throw new NotSupportedException($"Object type {o.GetType()} is not supported here.");
            }
        }
    }
}
