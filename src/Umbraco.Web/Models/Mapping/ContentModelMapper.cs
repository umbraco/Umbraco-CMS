using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentModelMapper : BaseContentModelMapper
    {
       
        public ContentModelMapper(ApplicationContext applicationContext, UserModelMapper userMapper)
            : base(applicationContext, userMapper)
        {
        }

        public ContentItemDto<IContent> ToContentItemDto(IContent content)
        {
            var result = base.ToContentItemDtoBase<IContent>(content);
            //NOTE: we don't need this for the dto and it's an extra lookup
            //result.ContentTypeAlias = content.ContentType.Alias;
            //result.Icon = content.ContentType.Icon;            
            //result.Updator = userMapper.ToUserBasic(content.GetWriterProfile());
            return result;            
        }

        public ContentItemBasic<ContentPropertyBasic, IContent> ToContentItemSimple(IContent content)
        {
            var result = base.ToContentItemSimpleBase<IContent>(content);
            result.ContentTypeAlias = content.ContentType.Alias;
            result.Icon = content.ContentType.Icon;
            result.Updator = UserMapper.ToUserBasic(content.GetWriterProfile());
            return result;
        } 

        public ContentItemDisplay ToContentItemDisplay(IContent content)
        {
            //create the list of tabs for properties assigned to tabs.
            var tabs = GetTabs(content);
            
            var result = CreateContent<ContentItemDisplay, ContentPropertyDisplay, IContent>(content, (display, originalContent) =>
                {
                    //fill in the rest
                    display.Updator = UserMapper.ToUserBasic(content.GetWriterProfile());
                    display.ContentTypeAlias = content.ContentType.Alias;
                    display.Icon = content.ContentType.Icon;

                    //set display props after the normal properties are alraedy mapped
                    display.Name = originalContent.Name;
                    display.Tabs = tabs;                    
                    //look up the published version of this item if it is not published
                    if (content.Published)
                    {
                        display.PublishDate = content.UpdateDate;
                    }
                    else if (content.HasPublishedVersion())
                    {
                        var published = ApplicationContext.Services.ContentService.GetPublishedVersion(content.Id);
                        display.PublishDate = published.UpdateDate;
                    }
                    else
                    {
                        display.PublishDate = null;
                    }
                    
                }, null, false);

            return result;
        }
        
    }
}
