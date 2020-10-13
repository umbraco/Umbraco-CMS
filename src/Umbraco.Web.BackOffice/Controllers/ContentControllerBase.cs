﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// An abstract base controller used for media/content/members to try to reduce code replication.
    /// </summary>
    [JsonDateTimeFormat]
    public abstract class ContentControllerBase : BackOfficeNotificationsController
    {
        protected ICultureDictionary CultureDictionary { get; }
        protected ILoggerFactory LoggerFactory { get; }
        protected IShortStringHelper ShortStringHelper { get; }
        protected IEventMessagesFactory EventMessages { get; }
        protected ILocalizedTextService LocalizedTextService { get; }
        private readonly ILogger<ContentControllerBase> _logger;

        protected ContentControllerBase(
            ICultureDictionary cultureDictionary,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEventMessagesFactory eventMessages,
            ILocalizedTextService localizedTextService)
        {
            CultureDictionary = cultureDictionary;
            LoggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ContentControllerBase>();
            ShortStringHelper = shortStringHelper;
            EventMessages = eventMessages;
            LocalizedTextService = localizedTextService;
        }

        protected NotFoundObjectResult HandleContentNotFound(object id, bool throwException = true)
        {
            ModelState.AddModelError("id", $"content with id: {id} was not found");
            var errorResponse = NotFound(ModelState);
            if (throwException)
            {
                throw new HttpResponseException(errorResponse);
            }
            return errorResponse;
        }

        /// <summary>
        /// Maps the dto property values to the persisted model
        /// </summary>
        internal void MapPropertyValuesForPersistence<TPersisted, TSaved>(
            TSaved contentItem,
            ContentPropertyCollectionDto dto,
            Func<TSaved, IProperty, object> getPropertyValue,
            Action<TSaved, IProperty, object> savePropertyValue,
            string culture)
            where TPersisted : IContentBase
            where TSaved : IContentSave<TPersisted>
        {
            // map the property values
            foreach (var propertyDto in dto.Properties)
            {
                // get the property editor
                if (propertyDto.PropertyEditor == null)
                {
                    _logger.LogWarning("No property editor found for property {PropertyAlias}", propertyDto.Alias);
                    continue;
                }

                // get the value editor
                // nothing to save/map if it is readonly
                var valueEditor = propertyDto.PropertyEditor.GetValueEditor();
                if (valueEditor.IsReadOnly) continue;

                // get the property
                var property = contentItem.PersistedContent.Properties[propertyDto.Alias];

                // prepare files, if any matching property and culture
                var files = contentItem.UploadedFiles
                    .Where(x => x.PropertyAlias == propertyDto.Alias && x.Culture == propertyDto.Culture && x.Segment == propertyDto.Segment)
                    .ToArray();

                foreach (var file in files)
                    file.FileName = file.FileName.ToSafeFileName(ShortStringHelper);

                // create the property data for the property editor
                var data = new ContentPropertyData(propertyDto.Value, propertyDto.DataType.Configuration)
                {
                    ContentKey = contentItem.PersistedContent.Key,
                    PropertyTypeKey = property.PropertyType.Key,
                    Files =  files
                };

                // let the editor convert the value that was received, deal with files, etc
                var value = valueEditor.FromEditor(data, getPropertyValue(contentItem, property));

                // set the value - tags are special
                var tagAttribute = propertyDto.PropertyEditor.GetTagAttribute();
                if (tagAttribute != null)
                {
                    var tagConfiguration = ConfigurationEditor.ConfigurationAs<TagConfiguration>(propertyDto.DataType.Configuration);
                    if (tagConfiguration.Delimiter == default) tagConfiguration.Delimiter = tagAttribute.Delimiter;
                    var tagCulture = property.PropertyType.VariesByCulture() ? culture : null;
                    property.SetTagsValue(value, tagConfiguration, tagCulture);
                }
                else
                    savePropertyValue(contentItem, property, value);
            }
        }

        protected virtual void HandleInvalidModelState(IErrorModel display)
        {
            //lastly, if it is not valid, add the model state to the outgoing object and throw a 403
            if (!ModelState.IsValid)
            {
                display.Errors = ModelState.ToErrorDictionary();
                throw HttpResponseException.CreateValidationErrorResponse(display);
            }
        }

        /// <summary>
        /// A helper method to attempt to get the instance from the request storage if it can be found there,
        /// otherwise gets it from the callback specified
        /// </summary>
        /// <typeparam name="TPersisted"></typeparam>
        /// <param name="getFromService"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is useful for when filters have already looked up a persisted entity and we don't want to have
        /// to look it up again.
        /// </remarks>
        protected TPersisted GetObjectFromRequest<TPersisted>(Func<TPersisted> getFromService)
        {
            //checks if the request contains the key and the item is not null, if that is the case, return it from the request, otherwise return
            // it from the callback
            return HttpContext.Items.ContainsKey(typeof(TPersisted).ToString()) && HttpContext.Items[typeof(TPersisted).ToString()] != null
                ? (TPersisted) HttpContext.Items[typeof (TPersisted).ToString()]
                : getFromService();
        }

        /// <summary>
        /// Returns true if the action passed in means we need to create something new
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static bool IsCreatingAction(ContentSaveAction action)
        {
            return (action.ToString().EndsWith("New"));
        }

        protected void AddCancelMessage(INotificationModel display,
            string header = "speechBubbles/operationCancelledHeader",
            string message = "speechBubbles/operationCancelledText",
            bool localizeHeader = true,
            bool localizeMessage = true,
            string[] headerParams = null,
            string[] messageParams = null)
        {
            //if there's already a default event message, don't add our default one
            var msgs = EventMessages;
            if (msgs != null && msgs.GetOrDefault().GetAll().Any(x => x.IsDefaultEventMessage)) return;

            display.AddWarningNotification(
                localizeHeader ? LocalizedTextService.Localize(header, headerParams) : header,
                localizeMessage ? LocalizedTextService.Localize(message, messageParams): message);
        }
    }
}
