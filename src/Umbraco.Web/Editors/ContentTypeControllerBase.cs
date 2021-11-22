using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    public abstract class ContentTypeControllerBase<TContentType> : UmbracoAuthorizedJsonController
        where TContentType : class, IContentTypeComposition
    {
        private readonly ICultureDictionaryFactory _cultureDictionaryFactory;
        private ICultureDictionary _cultureDictionary;

        protected ContentTypeControllerBase(ICultureDictionaryFactory cultureDictionaryFactory, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _cultureDictionaryFactory = cultureDictionaryFactory;
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
        /// <param name="isElement">Whether the composite content types should be applicable for an element type</param>
        /// <returns></returns>
        protected IEnumerable<Tuple<EntityBasic, bool>> PerformGetAvailableCompositeContentTypes(int contentTypeId,
            UmbracoObjectTypes type,
            string[] filterContentTypes,
            string[] filterPropertyTypes,
            bool isElement)
        {
            IContentTypeComposition source = null;

            //below is all ported from the old doc type editor and comes with the same weaknesses /insanity / magic

            IContentTypeComposition[] allContentTypes;

            switch (type)
            {
                case UmbracoObjectTypes.DocumentType:
                    if (contentTypeId > 0)
                    {
                        source = Services.ContentTypeService.Get(contentTypeId);
                        if (source == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                    }
                    allContentTypes = Services.ContentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
                    break;

                case UmbracoObjectTypes.MediaType:
                    if (contentTypeId > 0)
                    {
                        source = Services.MediaTypeService.Get(contentTypeId);
                        if (source == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                    }
                    allContentTypes = Services.MediaTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
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

            var availableCompositions = Services.ContentTypeService.GetAvailableCompositeContentTypes(source, allContentTypes, filterContentTypes, filterPropertyTypes, isElement);



            var currCompositions = source == null ? new IContentTypeComposition[] { } : source.ContentTypeComposition.ToArray();
            var compAliases = currCompositions.Select(x => x.Alias).ToArray();
            var ancestors = availableCompositions.Ancestors.Select(x => x.Alias);

            return availableCompositions.Results
                .Select(x => new Tuple<EntityBasic, bool>(Mapper.Map<IContentTypeComposition, EntityBasic>(x.Composition), x.Allowed))
                .Select(x =>
                {
                    //we need to ensure that the item is enabled if it is already selected
                    // but do not allow it if it is any of the ancestors
                    if (compAliases.Contains(x.Item1.Alias) && ancestors.Contains(x.Item1.Alias) == false)
                    {
                        //re-set x to be allowed (NOTE: I didn't know you could set an enumerable item in a lambda!)
                        x = new Tuple<EntityBasic, bool>(x.Item1, true);
                    }

                    //translate the name
                    x.Item1.Name = TranslateItem(x.Item1.Name);

                    var contentType = allContentTypes.FirstOrDefault(c => c.Key == x.Item1.Key);
                    var containers = GetEntityContainers(contentType, type)?.ToArray();
                    var containerPath = $"/{(containers != null && containers.Any() ? $"{string.Join("/", containers.Select(c => c.Name))}/" : null)}";
                    x.Item1.AdditionalData["containerPath"] = containerPath;

                    return x;
                })
                .ToList();
        }

        private IEnumerable<EntityContainer> GetEntityContainers(IContentTypeComposition contentType, UmbracoObjectTypes type)
        {
            if (contentType == null)
            {
                return null;
            }

            switch (type)
            {
                case UmbracoObjectTypes.DocumentType:
                    return Services.ContentTypeService.GetContainers(contentType as IContentType);
                case UmbracoObjectTypes.MediaType:
                    return Services.MediaTypeService.GetContainers(contentType as IMediaType);
                case UmbracoObjectTypes.MemberType:
                    return new EntityContainer[0];
                default:
                    throw new ArgumentOutOfRangeException("The entity type was not a content type");
            }
        }

        /// <summary>
        /// Returns a list of content types where a particular composition content type is used
        /// </summary>
        /// <param name="type">Type of content Type, eg documentType or mediaType</param>
        /// <param name="contentTypeId">Id of composition content type</param>
        /// <returns></returns>
        protected IEnumerable<EntityBasic> PerformGetWhereCompositionIsUsedInContentTypes(int contentTypeId, UmbracoObjectTypes type)
        {
            var id = 0;

            if (contentTypeId > 0)
            {
                IContentTypeComposition source;

                switch (type)
                {
                    case UmbracoObjectTypes.DocumentType:
                        source = Services.ContentTypeService.Get(contentTypeId);
                        break;

                    case UmbracoObjectTypes.MediaType:
                        source = Services.MediaTypeService.Get(contentTypeId);
                        break;

                    case UmbracoObjectTypes.MemberType:
                        source = Services.MemberTypeService.Get(contentTypeId);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }

                if (source == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                id = source.Id;
            }

            IEnumerable<IContentTypeComposition> composedOf;

            switch (type)
            {
                case UmbracoObjectTypes.DocumentType:
                    composedOf = Services.ContentTypeService.GetComposedOf(id);
                    break;

                case UmbracoObjectTypes.MediaType:
                    composedOf = Services.MediaTypeService.GetComposedOf(id);
                    break;

                case UmbracoObjectTypes.MemberType:
                    composedOf = Services.MemberTypeService.GetComposedOf(id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            EntityBasic TranslateName(EntityBasic e)
            {
                e.Name = TranslateItem(e.Name);
                return e;
            }

            return composedOf
                .Select(Mapper.Map<IContentTypeComposition, EntityBasic>)
                .Select(TranslateName)
                .ToList();
        }

        protected string TranslateItem(string text)
        {
            if (text == null)
                return null;

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return CultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        protected TContentType PerformPostSave<TContentTypeDisplay, TContentTypeSave, TPropertyType>(
            TContentTypeSave contentTypeSave,
            Func<int, TContentType> getContentType,
            Action<TContentType> saveContentType,
            Action<TContentTypeSave> beforeCreateNew = null)
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
            if (exists && (ctId == 0 || !ct.Alias.InvariantEquals(contentTypeSave.Alias)))
            {
                ModelState.AddModelError("Alias", Services.TextService.Localize("editcontenttype", "aliasAlreadyExists"));
            }

            // execute the external validators
            EditorValidator.Validate(ModelState, contentTypeSave);

            if (ModelState.IsValid == false)
            {
                throw CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay>(ctId, contentTypeSave, ct);
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
                    var responseEx = CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave, TPropertyType>(ex, contentTypeSave, ct, ctId);
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
                var allowIfselfAsChildSortOrder = -1;
                if (contentTypeSave.AllowedContentTypes != null)
                {
                    allowIfselfAsChildSortOrder = contentTypeSave.AllowedContentTypes.IndexOf(0);
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
                    var responseEx = CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave, TPropertyType>(ex, contentTypeSave, ct, ctId);
                    throw responseEx ?? ex;
                }

                var exResult = CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(contentTypeSave, newCt);
                if (exResult != null) throw exResult;

                //set id to null to ensure its handled as a new type
                contentTypeSave.Id = null;
                contentTypeSave.CreateDate = DateTime.Now;
                contentTypeSave.UpdateDate = DateTime.Now;

                saveContentType(newCt);

                //we need to save it twice to allow itself under itself.
                if (allowItselfAsChild && newCt != null)
                {
                    newCt.AllowedContentTypes =
                        newCt.AllowedContentTypes.Union(
                            new []{ new ContentTypeSort(newCt.Id, allowIfselfAsChildSortOrder) }
                        );
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
        protected HttpResponseMessage PerformMove(
            MoveOrCopy move,
            Func<int, TContentType> getContentType,
            Func<TContentType, int, Attempt<OperationResult<MoveOperationStatusType>>> doMove)
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
                response.Content = new StringContent(toMove.Path, Encoding.UTF8, "text/plain");
                return response;
            }

            switch (result.Result.Result)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedByPath"), "");
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
        protected HttpResponseMessage PerformCopy(
            MoveOrCopy move,
            Func<int, TContentType> getContentType,
            Func<TContentType, int, Attempt<OperationResult<MoveOperationStatusType, TContentType>>> doCopy)
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
                response.Content = new StringContent(copy.Path, Encoding.UTF8, "text/plain");
                return response;
            }

            switch (result.Result.Result)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedByPath"), "");
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
        private HttpResponseException CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(TContentTypeSave contentTypeSave, TContentType composition)
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
            where TContentTypeDisplay : ContentTypeCompositionDisplay
        {
            var service = Services.GetContentTypeService<TContentType>();
            var validateAttempt = service.ValidateComposition(composition);
            if (validateAttempt == false)
            {
                // if it's not successful then we need to return some model state for the property type and property group
                // aliases that are duplicated
                var duplicatePropertyTypeAliases = validateAttempt.Result.Distinct();
                var invalidPropertyGroupAliases = (validateAttempt.Exception as InvalidCompositionException)?.PropertyGroupAliases ?? Array.Empty<string>();

                AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(contentTypeSave, duplicatePropertyTypeAliases, invalidPropertyGroupAliases);

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
        /// <param name="duplicatePropertyTypeAliases"></param>
        /// <param name="invalidPropertyGroupAliases"></param>
        /// <returns></returns>
        private void AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(TContentTypeSave contentTypeSave, IEnumerable<string> duplicatePropertyTypeAliases, IEnumerable<string> invalidPropertyGroupAliases)
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
        {
            foreach (var propertyTypeAlias in duplicatePropertyTypeAliases)
            {
                // Find the property type relating to these
                var property = contentTypeSave.Groups.SelectMany(x => x.Properties).Single(x => x.Alias == propertyTypeAlias);
                var group = contentTypeSave.Groups.Single(x => x.Properties.Contains(property));
                var propertyIndex = group.Properties.IndexOf(property);
                var groupIndex = contentTypeSave.Groups.IndexOf(group);

                var key = $"Groups[{groupIndex}].Properties[{propertyIndex}].Alias";
                ModelState.AddModelError(key, "Duplicate property aliases aren't allowed between compositions");
            }

            foreach (var propertyGroupAlias in invalidPropertyGroupAliases)
            {
                // Find the property group relating to these
                var group = contentTypeSave.Groups.Single(x => x.Alias == propertyGroupAlias);
                var groupIndex = contentTypeSave.Groups.IndexOf(group);
                var key = $"Groups[{groupIndex}].Name";
                ModelState.AddModelError(key, "Different group types aren't allowed between compositions");
            }
        }

        /// <summary>
        /// If the exception is an InvalidCompositionException create a response exception to be thrown for validation errors
        /// </summary>
        /// <typeparam name="TContentTypeDisplay"></typeparam>
        /// <typeparam name="TContentTypeSave"></typeparam>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="ex"></param>
        /// <param name="contentTypeSave"></param>
        /// <param name="ct"></param>
        /// <param name="ctId"></param>
        /// <returns></returns>
        private HttpResponseException CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave, TPropertyType>(
            Exception ex, TContentTypeSave contentTypeSave, TContentType ct, int ctId)
            where TContentTypeDisplay : ContentTypeCompositionDisplay
            where TContentTypeSave : ContentTypeSave<TPropertyType>
            where TPropertyType : PropertyTypeBasic
        {
            InvalidCompositionException invalidCompositionException = null;
            if (ex is InvalidCompositionException)
            {
                invalidCompositionException = (InvalidCompositionException)ex;
            }
            else if (ex.InnerException is InvalidCompositionException)
            {
                invalidCompositionException = (InvalidCompositionException)ex.InnerException;
            }
            if (invalidCompositionException != null)
            {
                AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(contentTypeSave, invalidCompositionException.PropertyTypeAliases, invalidCompositionException.PropertyGroupAliases);
                return CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay>(ctId, contentTypeSave, ct);
            }
            return null;
        }

        /// <summary>
        /// Used to throw the ModelState validation results when the ModelState is invalid
        /// </summary>
        /// <typeparam name="TContentTypeDisplay"></typeparam>
        /// <typeparam name="TContentTypeSave"></typeparam>
        /// <param name="ctId"></param>
        /// <param name="contentTypeSave"></param>
        /// <param name="ct"></param>
        private HttpResponseException CreateModelStateValidationException<TContentTypeSave, TContentTypeDisplay>(int ctId, TContentTypeSave contentTypeSave, TContentType ct)
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
            => _cultureDictionary ?? (_cultureDictionary = _cultureDictionaryFactory.CreateDictionary());
    }
}
