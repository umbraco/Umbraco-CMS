using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     An API controller used for dealing with content types
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
[ParameterSwapControllerActionSelector(nameof(GetAllowedChildren), "contentId", typeof(int), typeof(Guid), typeof(Udi))]
public class MediaTypeController : ContentTypeControllerBase<IMediaType>
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    // TODO: Split this controller apart so that authz is consistent, currently we need to authz each action individually.
    // It would be possible to have something like a MediaTypeInfoController for the GetById/GetAllowedChildren/etc... actions

    private readonly IContentTypeService _contentTypeService;
    private readonly IEntityService _entityService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMediaService _mediaService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    public MediaTypeController(
        ICultureDictionary cultureDictionary,
        EditorValidatorCollection editorValidatorCollection,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IUmbracoMapper umbracoMapper,
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
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _localizedTextService =
            localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
    }

    public int GetCount() => _contentTypeService.Count();

    /// <summary>
    ///     Gets the media type a given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    public ActionResult<MediaTypeDisplay?> GetById(int id)
    {
        IMediaType? ct = _mediaTypeService.Get(id);
        if (ct == null)
        {
            return NotFound();
        }

        MediaTypeDisplay? dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(ct);
        return dto;
    }

    /// <summary>
    ///     Gets the media type a given guid
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    public ActionResult<MediaTypeDisplay?> GetById(Guid id)
    {
        IMediaType? mediaType = _mediaTypeService.Get(id);
        if (mediaType == null)
        {
            return NotFound();
        }

        MediaTypeDisplay? dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
        return dto;
    }

    /// <summary>
    ///     Gets the media type a given udi
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    public ActionResult<MediaTypeDisplay?> GetById(Udi id)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi == null)
        {
            return NotFound();
        }

        IMediaType? mediaType = _mediaTypeService.Get(guidUdi.Guid);
        if (mediaType == null)
        {
            return NotFound();
        }

        MediaTypeDisplay? dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
        return dto;
    }

    /// <summary>
    ///     Deletes a media type with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult DeleteById(int id)
    {
        IMediaType? foundType = _mediaTypeService.Get(id);
        if (foundType == null)
        {
            return NotFound();
        }

        _mediaTypeService.Delete(foundType, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);
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
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult GetAvailableCompositeMediaTypes(GetAvailableCompositionsFilter filter)
    {
        ActionResult<IEnumerable<Tuple<EntityBasic?, bool>>> actionResult = PerformGetAvailableCompositeContentTypes(
            filter.ContentTypeId,
            UmbracoObjectTypes.MediaType,
            filter.FilterContentTypes,
            filter.FilterPropertyTypes,
            filter.IsElement);

        if (!(actionResult.Result is null))
        {
            return actionResult.Result;
        }

        var result = actionResult.Value?.Select(x => new { contentType = x.Item1, allowed = x.Item2 });
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
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult GetWhereCompositionIsUsedInContentTypes(GetAvailableCompositionsFilter filter)
    {
        var result =
            PerformGetWhereCompositionIsUsedInContentTypes(filter.ContentTypeId, UmbracoObjectTypes.MediaType).Value?
                .Select(x => new { contentType = x });
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public MediaTypeDisplay? GetEmpty(int parentId)
    {
        IMediaType mt;
        if (parentId != Constants.System.Root)
        {
            IMediaType? parent = _mediaTypeService.Get(parentId);
            mt = parent != null
                ? new MediaType(_shortStringHelper, parent, string.Empty)
                : new MediaType(_shortStringHelper, parentId);
        }
        else
        {
            mt = new MediaType(_shortStringHelper, parentId);
        }

        mt.Icon = Constants.Icons.MediaImage;

        MediaTypeDisplay? dto = _umbracoMapper.Map<IMediaType, MediaTypeDisplay>(mt);
        return dto;
    }


    /// <summary>
    ///     Returns all media types
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IEnumerable<ContentTypeBasic> GetAll() =>
        _mediaTypeService.GetAll()
            .Select(_umbracoMapper.Map<IMediaType, ContentTypeBasic>).WhereNotNull();

    /// <summary>
    ///     Deletes a media type container with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult DeleteContainer(int id)
    {
        _mediaTypeService.DeleteContainer(id, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

        return Ok();
    }

    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult PostCreateContainer(int parentId, string name)
    {
        Attempt<OperationResult<OperationResultType, EntityContainer>?> result =
            _mediaTypeService.CreateContainer(parentId, Guid.NewGuid(), name, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

        if (result.Success)
        {
            return Ok(result.Result); //return the id
        }

        return ValidationProblem(result.Exception?.Message);
    }

    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult PostRenameContainer(int id, string name)
    {
        Attempt<OperationResult<OperationResultType, EntityContainer>?> result =
            _mediaTypeService.RenameContainer(id, name, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

        if (result.Success)
        {
            return Ok(result.Result); //return the id
        }

        return ValidationProblem(result.Exception?.Message);
    }

    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public ActionResult<MediaTypeDisplay?> PostSave(MediaTypeSave contentTypeSave)
    {
        ActionResult<IMediaType?> savedCt = PerformPostSave<MediaTypeDisplay, MediaTypeSave, PropertyTypeBasic>(
            contentTypeSave,
            i => _mediaTypeService.Get(i),
            type => _mediaTypeService.Save(type));

        if (!(savedCt.Result is null))
        {
            return savedCt.Result;
        }

        MediaTypeDisplay? display = _umbracoMapper.Map<MediaTypeDisplay>(savedCt.Value);


        display?.AddSuccessNotification(
            _localizedTextService.Localize("speechBubbles", "mediaTypeSavedHeader"),
            string.Empty);

        return display;
    }

    /// <summary>
    ///     Move the media type
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult PostMove(MoveOrCopy move) =>
        PerformMove(
            move,
            i => _mediaTypeService.Get(i),
            (type, i) => _mediaTypeService.Move(type, i));

    /// <summary>
    ///     Copy the media type
    /// </summary>
    /// <param name="copy"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    public IActionResult PostCopy(MoveOrCopy copy) =>
        PerformCopy(
            copy,
            i => _mediaTypeService.Get(i),
            (type, i) => _mediaTypeService.Copy(type, i));


    #region GetAllowedChildren

    /// <summary>
    ///     Returns the allowed child content type objects for the content item id passed in - based on an INT id
    /// </summary>
    /// <param name="contentId"></param>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    [OutgoingEditorModelEvent]
    public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
    {
        if (contentId == Constants.System.RecycleBinContent)
        {
            return Enumerable.Empty<ContentTypeBasic>();
        }

        IEnumerable<IMediaType> types;
        if (contentId == Constants.System.Root)
        {
            types = _mediaTypeService.GetAll().ToList();

            //if no allowed root types are set, just return everything
            if (types.Any(x => x.AllowedAsRoot))
            {
                types = types.Where(x => x.AllowedAsRoot);
            }
        }
        else
        {
            IMedia? contentItem = _mediaService.GetById(contentId);
            if (contentItem == null)
            {
                return Enumerable.Empty<ContentTypeBasic>();
            }

            IMediaType? contentType = _mediaTypeService.Get(contentItem.ContentTypeId);
            var ids = contentType?.AllowedContentTypes?.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

            if (ids is null || ids.Any() == false)
            {
                return Enumerable.Empty<ContentTypeBasic>();
            }

            types = _mediaTypeService.GetAll(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
        }

        var basics = types.Select(_umbracoMapper.Map<IMediaType, ContentTypeBasic>).WhereNotNull().ToList();

        foreach (ContentTypeBasic basic in basics)
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
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    public ActionResult<IEnumerable<ContentTypeBasic>> GetAllowedChildren(Guid contentId)
    {
        IEntitySlim? entity = _entityService.Get(contentId);
        if (entity != null)
        {
            return new ActionResult<IEnumerable<ContentTypeBasic>>(GetAllowedChildren(entity.Id));
        }

        return NotFound();
    }

    /// <summary>
    ///     Returns the allowed child content type objects for the content item id passed in - based on a UDI id
    /// </summary>
    /// <param name="contentId"></param>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
    public ActionResult<IEnumerable<ContentTypeBasic>> GetAllowedChildren(Udi contentId)
    {
        var guidUdi = contentId as GuidUdi;
        if (guidUdi != null)
        {
            IEntitySlim? entity = _entityService.Get(guidUdi.Guid);
            if (entity != null)
            {
                return new ActionResult<IEnumerable<ContentTypeBasic>>(GetAllowedChildren(entity.Id));
            }
        }

        return NotFound();
    }

    #endregion
}
