using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    internal class ContentItemFormatter : ContentItemBaseFormatter<IContent, ContentItemSave>
    {
        public ContentItemFormatter()
            : this(Core.ApplicationContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationContext"></param>
        public ContentItemFormatter(ApplicationContext applicationContext)
            : base(applicationContext)
        {
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return ApplicationContext.Services.ContentService.GetById(Convert.ToInt32(model.Id));
        }

        protected override IContent CreateNew(ContentItemSave model)
        {
            var contentType = ApplicationContext.Services.ContentTypeService.GetContentType(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found wth alias " + model.ContentTypeAlias);
            }
            return new Content(model.Name, model.ParentId, contentType);
        }

        protected override ContentItemDto<IContent> MapFromPersisted(ContentItemSave model)
        {
            return Mapper.Map<IContent, ContentItemDto<IContent>>(model.PersistedContent);
        }
    }
}