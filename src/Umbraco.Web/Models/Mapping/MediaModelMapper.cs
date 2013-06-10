using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MediaModelMapper : BaseContentModelMapper
    {
        public MediaModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
            : base(applicationContext, profileMapper)
        {
        }

        public MediaItemDisplay ToMediaItemDisplay(IMedia media)
        {
            //create the list of tabs for properties assigned to tabs.
            var tabs = GetTabs(media);

            var result = CreateContent<MediaItemDisplay, ContentPropertyDisplay>(media, (display, originalContent) =>
            {
                //fill in the rest
                display.ContentTypeAlias = media.ContentType.Alias;
                display.Icon = media.ContentType.Icon;

                //set display props after the normal properties are alraedy mapped
                display.Name = originalContent.Name;
                display.Tabs = tabs;
            }, null, false);

            return result;
        }

    }
}
