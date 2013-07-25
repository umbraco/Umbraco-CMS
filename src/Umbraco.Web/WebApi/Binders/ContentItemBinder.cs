using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Binders
{
    internal class ContentItemBinder : ContentItemBaseBinder<IContent>
    {

        public ContentItemBinder(ApplicationContext applicationContext) 
            : base(applicationContext)
        {     
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentItemBinder()
            : this(ApplicationContext.Current)
        {            
        }

        protected override IContent GetExisting(ContentItemSave<IContent> model)
        {
            return ApplicationContext.Services.ContentService.GetById(model.Id);
        }

        protected override IContent CreateNew(ContentItemSave<IContent> model)
        {
            var contentType = ApplicationContext.Services.ContentTypeService.GetContentType(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found wth alias " + model.ContentTypeAlias);
            }
            return new Content(model.Name, model.ParentId, contentType);     
        }

        protected override ContentItemDto<IContent> MapFromPersisted(ContentItemSave<IContent> model)
        {
            return Mapper.Map<IContent, ContentItemDto<IContent>>(model.PersistedContent);
        }
    }
}