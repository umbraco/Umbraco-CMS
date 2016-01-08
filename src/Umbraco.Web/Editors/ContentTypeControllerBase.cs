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
        /// <returns></returns>
        protected IEnumerable<EntityBasic> PerformGetAvailableCompositeContentTypes(int contentTypeId, int parentId, UmbracoObjectTypes type)
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

            // note: there are many sanity checks missing here and there ;-((
            // make sure once and for all
            //if (allContentTypes.Any(x => x.ParentId > 0 && x.ContentTypeComposition.Any(y => y.Id == x.ParentId) == false))
            //    throw new Exception("A parent does not belong to a composition.");

            // find out if any content type uses this content type
            var isUsing = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == contentTypeId)).ToArray();
            if (isUsing.Length > 0)
            {
                //if already in use a composition, do not allow any composited types
                return new List<EntityBasic>();
            }

            // if it is not used then composition is possible
            // hashset guarantees unicity on Id
            var list = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            // usable types are those that are top-level
            var usableContentTypes = allContentTypes
                .Where(x => x.ContentTypeComposition.Any() == false).ToArray();
            foreach (var x in usableContentTypes)
                list.Add(x);

            // indirect types are those that we use, directly or indirectly
            var indirectContentTypes = GetIndirect(source).ToArray();
            foreach (var x in indirectContentTypes)
                list.Add(x);

            if (UmbracoConfig.For.UmbracoSettings().Content.EnableInheritedDocumentTypes)
            {
                // get the ancestorIds via the parent
                var ancestorIds = new int[0];
                if (parentId > 0)
                {
                    var parent = allContentTypes.FirstOrDefault(x => x.Id == parentId);
                    if (parent != null)
                        ancestorIds = parent.Path.Split(',').Select(int.Parse).ToArray();
                }

                // add all ancestors as compositions (since they are implicitly "compositions" by inheritance and should
                // be in the list even though they can't be deselected)
                foreach (var x in allContentTypes)
                    if (ancestorIds.Contains(x.Id))
                        list.Add(x);
            }
            return list
                .Where(x => x.Id != contentTypeId)
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<IContentTypeComposition, EntityBasic>)
                .Select(x =>
                {
                    x.Name = TranslateItem(x.Name);
                    return x;
                })
                .ToList();
        }

        private static IEnumerable<IContentTypeComposition> GetIndirect(IContentTypeComposition ctype)
        {
            // hashset guarantees unicity on Id
            var all = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            var stack = new Stack<IContentTypeComposition>();

            if (ctype != null)
            {
                foreach (var x in ctype.ContentTypeComposition)
                    stack.Push(x);
            }

            while (stack.Count > 0)
            {
                var x = stack.Pop();
                all.Add(x);
                foreach (var y in x.ContentTypeComposition)
                    stack.Push(y);
            }

            return all;
        }

        /// <summary>
        /// Validates the composition and adds errors to the model state if any are found then throws an error response if there are errors
        /// </summary>
        /// <param name="contentTypeSave"></param>
        /// <param name="composition"></param>
        /// <returns></returns>
        protected void ValidateComposition(ContentTypeSave contentTypeSave, IContentTypeComposition composition)
        {
            var validateAttempt = Services.ContentTypeService.ValidateComposition(composition);
            if (validateAttempt == false)
            {
                //if it's not successful then we need to return some model state for the property aliases that 
                // are duplicated
                var propertyAliases = validateAttempt.Result.Distinct();
                foreach (var propertyAlias in propertyAliases)
                {
                    //find the property relating to these
                    var prop = contentTypeSave.Groups.SelectMany(x => x.Properties).Single(x => x.Alias == propertyAlias);
                    var group = contentTypeSave.Groups.Single(x => x.Properties.Contains(prop));
                    var propIndex = group.Properties.IndexOf(prop);
                    var groupIndex = contentTypeSave.Groups.IndexOf(group);

                    var key = string.Format("Groups[{0}].Properties[{1}].Alias", groupIndex, propIndex);
                    ModelState.AddModelError(key, "Duplicate property aliases not allowed between compositions");
                }

                var display = Mapper.Map<ContentTypeDisplay>(composition);
                //map the 'save' data on top
                display = Mapper.Map(contentTypeSave, display);
                display.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
            }

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

        protected TContentType PerformPostSave<TContentType, TContentTypeDisplay>(
            ContentTypeSave contentTypeSave,
            Func<int, TContentType> getContentType,
            Action<TContentType> saveContentType,
            bool validateComposition = true,
            Action<ContentTypeSave> beforeCreateNew = null)
            where TContentType : IContentTypeComposition
            where TContentTypeDisplay : ContentTypeCompositionDisplay
        {
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            
            if (ModelState.IsValid == false)
            {
                var ct = getContentType(ctId);
                //Required data is invalid so we cannot continue
                var forDisplay = Mapper.Map<TContentTypeDisplay>(ct);
                //map the 'save' data on top
                forDisplay = Mapper.Map(contentTypeSave, forDisplay);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //filter out empty properties
            contentTypeSave.Groups = contentTypeSave.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentTypeSave.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }
            
            if (ctId > 0)
            {
                //its an update to an existing
                var found = getContentType(ctId);
                if (found == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                Mapper.Map(contentTypeSave, found);

                if (validateComposition)
                {
                    //NOTE: this throws an error response if it is not valid
                    ValidateComposition(contentTypeSave, found);
                }

                saveContentType(found);

                return found;
            }
            else
            {
                if (beforeCreateNew != null)
                {
                    beforeCreateNew(contentTypeSave);
                }

                //set id to null to ensure its handled as a new type
                contentTypeSave.Id = null;
                contentTypeSave.CreateDate = DateTime.Now;
                contentTypeSave.UpdateDate = DateTime.Now;

                //check if the type is trying to allow type 0 below itself - id zero refers to the currently unsaved type
                //always filter these 0 types out
                var allowItselfAsChild = false;
                if (contentTypeSave.AllowedContentTypes != null)
                {
                    allowItselfAsChild = contentTypeSave.AllowedContentTypes.Any(x => x == 0);
                    contentTypeSave.AllowedContentTypes = contentTypeSave.AllowedContentTypes.Where(x => x > 0).ToList();
                }

                //save as new
                var newCt = Mapper.Map<TContentType>(contentTypeSave);

                if (validateComposition)
                {
                    //NOTE: this throws an error response if it is not valid
                    ValidateComposition(contentTypeSave, newCt);
                }

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
        /// Change the sort order for media
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