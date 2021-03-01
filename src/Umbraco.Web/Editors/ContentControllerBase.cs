using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An abstract base controller used for media/content/members to try to reduce code replication.
    /// </summary>
    [JsonDateTimeFormatAttribute]
    public abstract class ContentControllerBase : BackOfficeNotificationsController
    {
        protected ContentControllerBase(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        protected HttpResponseMessage HandleContentNotFound(object id, bool throwException = true)
        {
            ModelState.AddModelError("id", $"content with id: {id} was not found");
            var errorResponse = Request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                ModelState);
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
            Func<TSaved, Property, object> getPropertyValue,
            Action<TSaved, Property, object> savePropertyValue,
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
                    Logger.Warn<ContentController, string>("No property editor found for property {PropertyAlias}", propertyDto.Alias);
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
                    file.FileName = file.FileName.ToSafeFileName();

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
                throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
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
            return Request.Properties.ContainsKey(typeof(TPersisted).ToString()) && Request.Properties[typeof(TPersisted).ToString()] != null
                ? (TPersisted) Request.Properties[typeof (TPersisted).ToString()]
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
            // TODO: inject
            var msgs = Current.EventMessages;
            if (msgs != null && msgs.GetAll().Any(x => x.IsDefaultEventMessage)) return;

            display.AddWarningNotification(
                localizeHeader ? Services.TextService.Localize(header, headerParams) : header,
                localizeMessage ? Services.TextService.Localize(message, messageParams): message);
        }
    }
}
