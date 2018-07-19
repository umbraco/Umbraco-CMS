using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors.Binders
{
    internal class ContentItemBinder : ContentItemBaseBinder<IContent, ContentItemSave>
    {
        public ContentItemBinder() : this(Current.Logger, Current.Services, Current.UmbracoContextAccessor)
        {
        }

        public ContentItemBinder(ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, services, umbracoContextAccessor)
        {
        }
        
        protected override IContent GetExisting(ContentItemSave model)
        {
            return Services.ContentService.GetById(model.Id);
        }

        protected override IContent CreateNew(ContentItemSave model)
        {
            var contentType = Services.ContentTypeService.Get(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
            }
            return new Content(
                model.PersistedContent.ContentType.VariesByCulture() ? null : model.Variants.First().Name,
                model.ParentId,
                contentType);
        }

        protected override ContentItemDto<IContent> MapFromPersisted(ContentItemSave model)
        {
            return MapFromPersisted(model.PersistedContent);
        }

        internal static ContentItemDto<IContent> MapFromPersisted(IContent content)
        {
            return Mapper.Map<ContentItemDto<IContent>>(content);
        }

        
    }
}
