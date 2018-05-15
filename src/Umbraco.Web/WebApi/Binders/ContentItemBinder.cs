using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi.Binders
{
    internal class ContentItemBinder : ContentItemBaseBinder<IContent, ContentItemSave>
    {
        protected override ContentItemValidationHelper<IContent, ContentItemSave> GetValidationHelper()
        {
            return new ContentValidationHelper();
        }

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
            return MapFromPersisted(model.PersistedContent, model.Culture);
        }

        internal static ContentItemDto<IContent> MapFromPersisted(IContent content, string culture)
        {
            return ContextMapper.Map<IContent, ContentItemDto<IContent>>(content, new Dictionary<string, object>
            {
                [ContextMapper.CultureKey] = culture
            });
        }

        internal class ContentValidationHelper : ContentItemValidationHelper<IContent, ContentItemSave>
        {
            /// <summary>
            /// Validates that the correct information is in the request for saving a culture variant
            /// </summary>
            /// <param name="postedItem"></param>
            /// <param name="actionContext"></param>
            /// <returns></returns>
            protected override bool ValidateCultureVariant(ContentItemSave postedItem, HttpActionContext actionContext)
            {
                var contentType = postedItem.PersistedContent.GetContentType();
                if (contentType.Variations.HasAny(Core.Models.ContentVariation.CultureNeutral | Core.Models.ContentVariation.CultureSegment)
                    && postedItem.Culture.IsNullOrWhiteSpace())
                {
                    //we cannot save a content item that is culture variant if no culture was specified in the request!
                    actionContext.Response = actionContext.Request.CreateValidationErrorResponse($"No 'Culture' found in request. Cannot save a content item that is of a {Core.Models.ContentVariation.CultureNeutral} content type without a specified culture.");
                    return false;
                }
                return true;
            }
        }
    }
}
