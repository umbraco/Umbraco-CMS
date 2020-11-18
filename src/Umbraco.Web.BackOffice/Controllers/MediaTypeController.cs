using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    // TODO:  We'll need to be careful about the security on this controller, when we start implementing
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    ///     An API controller used for dealing with content types
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    public class MediaTypeController : ContentTypeControllerBase<IMediaType>
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IEntityService _entityService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public MediaTypeController(ICultureDictionary cultureDictionary,
            EditorValidatorCollection editorValidatorCollection,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMemberTypeService memberTypeService,
            UmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IEntityService entityService,
            IMediaService mediaService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor)
            : base(
            cultureDictionary,
            editorValidatorCollection,
            contentTypeService,
            mediaTypeService,
            memberTypeService,
            umbracoMapper,
            localizedTextService)
        {
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _mediaTypeService = mediaTypeService ?? throw new ArgumentNullException(nameof(mediaTypeService));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _localizedTextService =
                localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        public int GetCount() => _contentTypeService.Count();

        /// <summary>
        /// Gets the media type a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(int id)
        {
            var ct = _mediaTypeService.Get(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Gets the media type a given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(Guid id)
        {
            var mediaType = _mediaTypeService.Get(id);
            if (mediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
            return dto;
        }

        /// <summary>
        /// Gets the media type a given udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var mediaType = _mediaTypeService.Get(guidUdi.Guid);
            if (mediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
            return dto;
        }

        /// <summary>
        ///     Deletes a media type with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var foundType = _mediaTypeService.Get(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            _mediaTypeService.Delete(foundType, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
            return Ok();
        }

        /// <summary>
        ///     Returns the available compositions for this content type
        ///     This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request
        ///     body
        /// </summary>
        /// <param name="filter.contentTypeId"></param>
        /// <param name="filter.ContentTypes">
        ///     This is normally an empty list but if additional content type aliases are passed in, any content types containing
        ///     those aliases will be filtered out
        ///     along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filter.PropertyTypes">
        ///     This is normally an empty list but if additional property type aliases are passed in, any content types that have
        ///     these aliases will be filtered out.
        ///     This is required because in the case of creating/modifying a content type because new property types being added to
        ///     it are not yet persisted so cannot
        ///     be looked up via the db, they need to be passed in.
        /// </param>
        /// <param name="filter">
        ///     Filter applied when resolving compositions
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetAvailableCompositeMediaTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId, UmbracoObjectTypes.MediaType,
                    filter.FilterContentTypes, filter.FilterPropertyTypes, filter.IsElement)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Ok(result);
        }

        /// <summary>
        ///     Returns where a particular composition has been used
        ///     This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request
        ///     body
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetWhereCompositionIsUsedInContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result =
                PerformGetWhereCompositionIsUsedInContentTypes(filter.ContentTypeId, UmbracoObjectTypes.MediaType)
                    .Select(x => new
                    {
                        contentType = x
                    });
            return Ok(result);
        }

        public MediaTypeDisplay GetEmpty(int parentId)
        {
            IMediaType mt;
            if (parentId != Constants.System.Root)
            {
                var parent = _mediaTypeService.Get(parentId);
                mt = parent != null
                    ? new MediaType(_shortStringHelper, parent, string.Empty)
                    : new MediaType(_shortStringHelper, parentId);
            }
            else
                mt = new MediaType(_shortStringHelper, parentId);

            mt.Icon = Constants.Icons.MediaImage;

            var dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mt);
            return dto;
        }


        /// <summary>
        ///     Returns all media types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll() =>
            _mediaTypeService.GetAll()
                .Select(_umbracoMapper.Map<IMediaType, ContentTypeBasic>);

        /// <summary>
        ///     Deletes a media type container with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteContainer(int id)
        {
            _mediaTypeService.DeleteContainer(id, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            return Ok();
        }

        public IActionResult PostCreateContainer(int parentId, string name)
        {
            var result = _mediaTypeService.CreateContainer(parentId, name, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            return result
                ? Ok(result.Result) //return the id
                : throw HttpResponseException.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public IActionResult PostRenameContainer(int id, string name)
        {
            var result = _mediaTypeService.RenameContainer(id, name, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            return result
                ? Ok(result.Result) //return the id
                : throw HttpResponseException.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public MediaTypeDisplay PostSave(MediaTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<MediaTypeDisplay, MediaTypeSave, PropertyTypeBasic>(
                contentTypeSave,
                i => _mediaTypeService.Get(i),
                type => _mediaTypeService.Save(type));

            var display = _umbracoMapper.Map<MediaTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                _localizedTextService.Localize("speechBubbles/mediaTypeSavedHeader"),
                string.Empty);

            return display;
        }

        /// <summary>
        ///     Move the media type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public IActionResult PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move,
                i => _mediaTypeService.Get(i),
                (type, i) => _mediaTypeService.Move(type, i));
        }

        /// <summary>
        ///     Copy the media type
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public IActionResult PostCopy(MoveOrCopy copy)
        {
            return PerformCopy(
                copy,
                i => _mediaTypeService.Get(i),
                (type, i) => _mediaTypeService.Copy(type, i));
        }


        #region GetAllowedChildren

        /// <summary>
        ///     Returns the allowed child content type objects for the content item id passed in - based on an INT id
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IMediaType> types;
            if (contentId == Constants.System.Root)
            {
                types = _mediaTypeService.GetAll().ToList();

                //if no allowed root types are set, just return everything
                if (types.Any(x => x.AllowedAsRoot))
                    types = types.Where(x => x.AllowedAsRoot);
            }
            else
            {
                var contentItem = _mediaService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var contentType = _mediaTypeService.Get(contentItem.ContentTypeId);
                var ids = contentType.AllowedContentTypes.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = _mediaTypeService.GetAll(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
            }

            var basics = types.Select(_umbracoMapper.Map<IMediaType, ContentTypeBasic>).ToList();

            foreach (var basic in basics)
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
            }

            return basics.OrderBy(c => contentId == Constants.System.Root ? c.Name : string.Empty);
        }

        /// <summary>
        ///     Returns the allowed child content type objects for the content item id passed in - based on a GUID id
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(Guid contentId)
        {
            var entity = _entityService.Get(contentId);
            if (entity != null)
            {
                return GetAllowedChildren(entity.Id);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        ///     Returns the allowed child content type objects for the content item id passed in - based on a UDI id
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi != null)
            {
                var entity = _entityService.Get(guidUdi.Guid);
                if (entity != null)
                {
                    return GetAllowedChildren(entity.Id);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        #endregion
    }
}
