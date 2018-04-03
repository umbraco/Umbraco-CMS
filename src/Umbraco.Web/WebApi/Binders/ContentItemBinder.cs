using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Binders
{
    internal class ContentItemBinder : ContentItemBaseBinder<IContent, ContentItemSave>
    {
        protected override IContent GetExisting(ContentItemSave model)
        {
            return Services.ContentService.GetById(Convert.ToInt32(model.Id));
        }

        protected override IContent CreateNew(ContentItemSave model)
        {
            var contentType = Services.ContentTypeService.Get(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
            }
            return new Content(model.Name, model.ParentId, contentType);
        }

        protected override ContentItemDto<IContent> MapFromPersisted(ContentItemSave model)
        {
            return ContextMapper.Map<IContent, ContentItemDto<IContent>>(model.PersistedContent, new Dictionary<string, object>
            {
                [ContextMapper.LanguageKey] = model.LanguageId
            });
        }
    }
}
