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

        public ContentItemDto<IMedia> ToMediaItemDto(IMedia content)
        {
            var result = base.ToContentItemDtoBase<IMedia>(content);
            //NOTE: we don't need this for the dto and it's an extra lookup
            //result.ContentTypeAlias = content.ContentType.Alias;
            //result.Icon = content.ContentType.Icon;            
            //result.Updator = ProfileMapper.ToBasicUser(content.GetWriterProfile());
            return result;
        }

        public ContentItemBasic<ContentPropertyBasic, IMedia> ToMediaItemSimple(IMedia content)
        {
            var result = base.ToContentItemSimpleBase<IMedia>(content);
            result.ContentTypeAlias = content.ContentType.Alias;
            result.Icon = content.ContentType.Icon;
            return result;
        } 

        public MediaItemDisplay ToMediaItemDisplay(IMedia media)
        {
            //create the list of tabs for properties assigned to tabs.
            var tabs = GetTabs(media);

            var result = CreateContent<MediaItemDisplay, ContentPropertyDisplay, IMedia>(media, (display, originalContent) =>
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
