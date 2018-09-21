using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    public class ContentInfoContentAppDefinition : IContentAppDefinition
    {
        // see note on ContentApp
        private const int Weight = +100;

        private ContentApp _contentApp;
        private ContentApp _mediaApp;

        public ContentApp GetContentAppFor(object o)
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

                default:
                    throw new NotSupportedException($"Object type {o.GetType()} is not supported here.");
            }
        }
    }
}
