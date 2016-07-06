using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An abstract base controller used for media/content (and probably members) to try to reduce code replication.
    /// </summary>
    [OutgoingDateTimeFormat]
    public abstract class ContentControllerBase : BackOfficeNotificationsController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected ContentControllerBase()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected ContentControllerBase(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        protected HttpResponseMessage HandleContentNotFound(object id, bool throwException = true)
        {
            ModelState.AddModelError("id", string.Format("content with id: {0} was not found", id));
            var errorResponse = Request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                ModelState);
            if (throwException)
            {
                throw new HttpResponseException(errorResponse);    
            }
            return errorResponse;
        }

        protected void UpdateName<TPersisted>(ContentBaseItemSave<TPersisted> contentItem) 
            where TPersisted : IContentBase
        {
            //Don't update the name if it is empty
            if (!contentItem.Name.IsNullOrWhiteSpace())
            {
                contentItem.PersistedContent.Name = contentItem.Name;
            }
        }

        protected HttpResponseMessage PerformSort(ContentSortOrder sorted)
        {
            if (sorted == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //if there's nothing to sort just return ok
            if (sorted.IdSortOrder.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            return null;
        }

        /// <summary>
        /// Maps the dto property values to the persisted model
        /// </summary>
        /// <typeparam name="TPersisted"></typeparam>
        /// <param name="contentItem"></param>
        protected virtual void MapPropertyValues<TPersisted>(ContentBaseItemSave<TPersisted> contentItem)
            where TPersisted : IContentBase
        {
            //Map the property values
            foreach (var p in contentItem.ContentDto.Properties)
            {
                //get the dbo property
                var dboProperty = contentItem.PersistedContent.Properties[p.Alias];

                //create the property data to send to the property editor
                var d = new Dictionary<string, object>();
                //add the files if any
                var files = contentItem.UploadedFiles.Where(x => x.PropertyAlias == p.Alias).ToArray();
                if (files.Length > 0)
                {
                    d.Add("files", files);
                }
                
                var data = new ContentPropertyData(p.Value, p.PreValues, d);

                //get the deserialized value from the property editor
                if (p.PropertyEditor == null)
                {
                    LogHelper.Warn<ContentController>("No property editor found for property " + p.Alias);
                }
                else
                {
                    var valueEditor = p.PropertyEditor.ValueEditor;
                    //don't persist any bound value if the editor is readonly
                    if (valueEditor.IsReadOnly == false)
                    {                        
                        var propVal = p.PropertyEditor.ValueEditor.ConvertEditorToDb(data, dboProperty.Value);
                        var supportTagsAttribute = TagExtractor.GetAttribute(p.PropertyEditor);
                        if (supportTagsAttribute != null)
                        {
                            TagExtractor.SetPropertyTags(dboProperty, data, propVal, supportTagsAttribute);
                        }
                        else
                        {
                            dboProperty.Value = propVal;
                        }
                    }    
                    
                }
            }
        }

        protected void HandleInvalidModelState<T, TPersisted>(ContentItemDisplayBase<T, TPersisted> display) 
            where TPersisted : IContentBase 
            where T : ContentPropertyBasic
        {
            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
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
        /// This is useful for when filters have alraedy looked up a persisted entity and we don't want to have
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
            bool localizeMessage = true)
        {
            //if there's already a default event message, don't add our default one
            var msgs = UmbracoContext.GetCurrentEventMessages();
            if (msgs != null && msgs.GetAll().Any(x => x.IsDefaultEventMessage)) return;

            display.AddWarningNotification(
                localizeHeader ? Services.TextService.Localize(header) : header,
                localizeMessage ? Services.TextService.Localize(message): message);
        }
    }
}
