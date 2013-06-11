using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Binders
{
    internal class ContentItemBinder : ContentItemBaseBinder<IContent>
    {
        private readonly ContentModelMapper _contentModelMapper;

        public ContentItemBinder(ApplicationContext applicationContext, ContentModelMapper contentModelMapper) 
            : base(applicationContext)
        {
            _contentModelMapper = contentModelMapper;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentItemBinder()
            : this(ApplicationContext.Current, new ContentModelMapper(ApplicationContext.Current, new ProfileModelMapper()))
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

        protected override ContentItemDto<IContent> Map(ContentItemSave<IContent> model)
        {
            return _contentModelMapper.ToContentItemDto(model.PersistedContent);
        }
    }
}