using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    public abstract class ContentTypeControllerBase : UmbracoAuthorizedJsonController
    {
        private ICultureDictionary _cultureDictionary;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ContentTypeControllerBase()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected ContentTypeControllerBase(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns the available composite content types for a given content type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filterContentTypes">
        /// This is normally an empty list but if additional content type aliases are passed in, any content types containing those aliases will be filtered out
        /// along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filterPropertyTypes">
        /// This is normally an empty list but if additional property type aliases are passed in, any content types that have these aliases will be filtered out.
        /// This is required because in the case of creating/modifying a content type because new property types being added to it are not yet persisted so cannot
        /// be looked up via the db, they need to be passed in.
        /// </param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        protected IEnumerable<Tuple<EntityBasic, bool>> PerformGetAvailableCompositeContentTypes(int contentTypeId,
            UmbracoObjectTypes type,
            string[] filterContentTypes,
            string[] filterPropertyTypes)
        {
            IContentTypeComposition source = null;

            //below is all ported from the old doc type editor and comes with the same weaknesses /insanity / magic

            IContentTypeComposition[] allContentTypes;

            switch (type)
            {
                case UmbracoObjectTypes.DocumentType:
                    if (contentTypeId > 0)
                    {
                        source = Services.ContentTypeService.GetContentType(contentTypeId);
                        if (source == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                    }
                    allContentTypes = Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>().ToArray();
                    break;

                case UmbracoObjectTypes.MediaType:
                    if (contentTypeId > 0)
                    {
                        source = Services.ContentTypeService.GetMediaType(contentTypeId);
                        if (source == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                    }
                    allContentTypes = Services.ContentTypeService.GetAllMediaTypes().Cast<IContentTypeComposition>().ToArray();
                    break;

                case UmbracoObjectTypes.MemberType:
                    if (contentTypeId > 0)
                    {
                        source = Services.MemberTypeService.Get(contentTypeId);
                        if (source == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                    }
                    allContentTypes = Services.MemberTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("The entity type was not a content type");
            }

            var availableCompositions = Services.ContentTypeService.GetAvailableCompositeContentTypes(source, allContentTypes, filterContentTypes, filterPropertyTypes);

            var currCompositions = source == null ? new IContentTypeComposition[] { } : source.ContentTypeComposition.ToArray();
            var compAliases = currCompositions.Select(x => x.Alias).ToArray();
            var ancestors = availableCompositions.Ancestors.Select(x => x.Alias);

            return availableCompositions.Results
                .Select(x => new Tuple<EntityBasic, bool>(Mapper.Map<IContentTypeComposition, EntityBasic>(x.Composition), x.Allowed))
                .Select(x =>
                {
                    //translate the name
                    x.Item1.Name = TranslateItem(x.Item1.Name);

                    //we need to ensure that the item is enabled if it is already selected
                    // but do not allow it if it is any of the ancestors
                    if (compAliases.Contains(x.Item1.Alias) && ancestors.Contains(x.Item1.Alias) == false)
                    {
                        //re-set x to be allowed (NOTE: I didn't know you could set an enumerable item in a lambda!)
                        x = new Tuple<EntityBasic, bool>(x.Item1, true);
                    }

                    return x;
                })
                .ToList();
        }


        protected string TranslateItem(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return CultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        protected TContentType PerformPostSave<TContentType, TContentTypeDisplay, TContentTypeSave, TPropertyType>(
            TContentTypeSave contentTypeSave,
            Func<int, TContentType> getContentType,
            Action<TContentType> saveContentType,
            Action<TContentTypeSave> beforeCreateNew = null)
            where TContentType : class, IContentTypeComposition
            where TContentTypeDisplay : ContentTypeCompositionDisplay
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
        {
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            var ct = ctId > 0 ? getContentType(ctId) : null;
            if (ctId > 0 && ct == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Validate that there's no other ct with the same alias
            // it in fact cannot be the same as any content type alias (member, content or media) because
            // this would interfere with how ModelsBuilder works and also how many of the published caches
            // works since that is based on aliases.
            var allAliases = Services.ContentTypeService.GetAllContentTypeAliases();
            var exists = allAliases.InvariantContains(contentTypeSave.Alias);
            if ((exists) && (ctId == 0 || ct.Alias != contentTypeSave.Alias))
            {
                ModelState.AddModelError("Alias", "A content type, media type or member type with this alias already exists");
            }

            //now let the external validators execute
            ValidationHelper.ValidateEditorModelWithResolver(ModelState, contentTypeSave);

            if (ModelState.IsValid == false)
            {
                throw CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay, TContentType>(ctId, contentTypeSave, ct);
            }

            //filter out empty properties
            contentTypeSave.Groups = contentTypeSave.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentTypeSave.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }

            if (ctId > 0)
            {
                //its an update to an existing content type

                //This mapping will cause a lot of content type validation to occur which we need to deal with
                try
                {
                    Mapper.Map(contentTypeSave, ct);
                }
                catch (Exception ex)
                {
                    var responseEx = CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentType, TContentTypeSave, TPropertyType>(ex, contentTypeSave, ct, ctId);
                    if (responseEx != null) throw responseEx;
                }

                var exResult = CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(contentTypeSave, ct);
                if (exResult != null) throw exResult;

                saveContentType(ct);

                return ct;
            }
            else
            {
                if (beforeCreateNew != null)
                {
                    beforeCreateNew(contentTypeSave);
                }

                //check if the type is trying to allow type 0 below itself - id zero refers to the currently unsaved type
                //always filter these 0 types out
                var allowItselfAsChild = false;
                if (contentTypeSave.AllowedContentTypes != null)
                {
                    allowItselfAsChild = contentTypeSave.AllowedContentTypes.Any(x => x == 0);
                    contentTypeSave.AllowedContentTypes = contentTypeSave.AllowedContentTypes.Where(x => x > 0).ToList();
                }

                //save as new

                TContentType newCt = null;
                try
                {
                    //This mapping will cause a lot of content type validation to occur which we need to deal with
                    newCt = Mapper.Map<TContentType>(contentTypeSave);
                }
                catch (Exception ex)
                {
                    var responseEx = CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentType, TContentTypeSave, TPropertyType>(ex, contentTypeSave, ct, ctId);
                    if (responseEx != null) throw responseEx;
                }

                var exResult = CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(contentTypeSave, newCt);
                if (exResult != null) throw exResult;

                //set id to null to ensure its handled as a new type
                contentTypeSave.Id = null;
                contentTypeSave.CreateDate = DateTime.Now;
                contentTypeSave.UpdateDate = DateTime.Now;

                saveContentType(newCt);

                //we need to save it twice to allow itself under itself.
                if (allowItselfAsChild)
                {
                    //NOTE: This will throw if the composition isn't right... but it shouldn't be at this stage
                    newCt.AddContentType(newCt);
                    saveContentType(newCt);
                }
                return newCt;
            }
        }

        /// <summary>
        /// Move
        /// </summary>
        /// <param name="move"></param>
        /// <param name="getContentType"></param>
        /// <param name="doMove"></param>
        /// <returns></returns>
        protected HttpResponseMessage PerformMove<TContentType>(
            MoveOrCopy move,
            Func<int, TContentType> getContentType,
            Func<TContentType, int, Attempt<OperationStatus<MoveOperationStatusType>>> doMove)
            where TContentType : IContentTypeComposition
        {
            var toMove = getContentType(move.Id);
            if (toMove == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var result = doMove(toMove, move.ParentId);
            if (result.Success)
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(toMove.Path, Encoding.UTF8, "application/json");
                return response;
            }

            switch (result.Result.StatusType)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    return Request.CreateValidationErrorResponse(notificationModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Move
        /// </summary>
        /// <param name="move"></param>
        /// <param name="getContentType"></param>
        /// <param name="doCopy"></param>
        /// <returns></returns>
        protected HttpResponseMessage PerformCopy<TContentType>(
            MoveOrCopy move,
            Func<int, TContentType> getContentType,
            Func<TContentType, int, Attempt<OperationStatus<TContentType, MoveOperationStatusType>>> doCopy)
            where TContentType : IContentTypeComposition
        {
            var toMove = getContentType(move.Id);
            if (toMove == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var result = doCopy(toMove, move.ParentId);
            if (result.Success)
            {
                var copy = result.Result.Entity;
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(copy.Path, Encoding.UTF8, "application/json");
                return response;
            }

            switch (result.Result.StatusType)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    return Request.CreateValidationErrorResponse(notificationModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Validates the composition and adds errors to the model state if any are found then throws an error response if there are errors
        /// </summary>
        /// <param name="contentTypeSave"></param>
        /// <param name="composition"></param>
        /// <returns></returns>
        private HttpResponseException CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(TContentTypeSave contentTypeSave, IContentTypeComposition composition)
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
            where TContentTypeDisplay : ContentTypeCompositionDisplay
        {
            var validateAttempt = Services.ContentTypeService.ValidateComposition(composition);
            if (validateAttempt == false)
            {
                //if it's not successful then we need to return some model state for the property aliases that
                // are duplicated
                var invalidPropertyAliases = validateAttempt.Result.Distinct();
                AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(contentTypeSave, invalidPropertyAliases);

                var display = Mapper.Map<TContentTypeDisplay>(composition);
                //map the 'save' data on top
                display = Mapper.Map(contentTypeSave, display);
                display.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
            }
            return null;
        }

        /// <summary>
        /// Adds errors to the model state if any invalid aliases are found then throws an error response if there are errors
        /// </summary>
        /// <param name="contentTypeSave"></param>
        /// <param name="invalidPropertyAliases"></param>
        /// <returns></returns>
        private void AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(TContentTypeSave contentTypeSave, IEnumerable<string> invalidPropertyAliases)
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
        {
            foreach (var propertyAlias in invalidPropertyAliases)
            {
                //find the property relating to these
                var prop = contentTypeSave.Groups.SelectMany(x => x.Properties).Single(x => x.Alias == propertyAlias);
                var group = contentTypeSave.Groups.Single(x => x.Properties.Contains(prop));

                var key = string.Format("Groups[{0}].Properties[{1}].Alias", group.SortOrder, prop.SortOrder);
                ModelState.AddModelError(key, "Duplicate property aliases not allowed between compositions");
            }
        }

        /// <summary>
        /// If the exception is an InvalidCompositionException create a response exception to be thrown for validation errors
        /// </summary>
        /// <typeparam name="TContentTypeDisplay"></typeparam>
        /// <typeparam name="TContentType"></typeparam>
        /// <typeparam name="TContentTypeSave"></typeparam>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="ex"></param>
        /// <param name="contentTypeSave"></param>
        /// <param name="ct"></param>
        /// <param name="ctId"></param>
        /// <returns></returns>
        private HttpResponseException CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentType, TContentTypeSave, TPropertyType>(
            Exception ex, TContentTypeSave contentTypeSave, TContentType ct, int ctId)
            where TContentType : class, IContentTypeComposition
            where TContentTypeDisplay : ContentTypeCompositionDisplay
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
        {
            InvalidCompositionException invalidCompositionException = null;
            if (ex is AutoMapperMappingException && ex.InnerException is InvalidCompositionException)
            {
                invalidCompositionException = (InvalidCompositionException)ex.InnerException;
            }
            else if (ex.InnerException is InvalidCompositionException)
            {
                invalidCompositionException = (InvalidCompositionException)ex;
            }
            if (invalidCompositionException != null)
            {
                AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(contentTypeSave, invalidCompositionException.PropertyTypeAliases);
                return CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay, TContentType>(ctId, contentTypeSave, ct);
            }
            return null;
        }

        /// <summary>
        /// Used to throw the ModelState validation results when the ModelState is invalid
        /// </summary>
        /// <typeparam name="TContentTypeDisplay"></typeparam>
        /// <typeparam name="TContentType"></typeparam>
        /// <typeparam name="TContentTypeSave"></typeparam>
        /// <param name="ctId"></param>
        /// <param name="contentTypeSave"></param>
        /// <param name="ct"></param>
        private HttpResponseException CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay, TContentType>(int ctId, TContentTypeSave contentTypeSave, TContentType ct)
            where TContentType : class, IContentTypeComposition
            where TContentTypeDisplay : ContentTypeCompositionDisplay
            where TContentTypeSave : ContentTypeSave
        {
            TContentTypeDisplay forDisplay;
            if (ctId > 0)
            {
                //Required data is invalid so we cannot continue
                forDisplay = Mapper.Map<TContentTypeDisplay>(ct);
                //map the 'save' data on top
                forDisplay = Mapper.Map(contentTypeSave, forDisplay);
            }
            else
            {
                //map the 'save' data to display
                forDisplay = Mapper.Map<TContentTypeDisplay>(contentTypeSave);
            }

            forDisplay.Errors = ModelState.ToErrorDictionary();
            return new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
        }

        private ICultureDictionary CultureDictionary
        {
            get
            {
                return
                    _cultureDictionary ??
                    (_cultureDictionary = CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary());
            }
        }


    }
}