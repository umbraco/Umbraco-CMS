using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     Am abstract API controller providing functionality used for dealing with content and media types
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[PrefixlessBodyModelValidator]
public abstract class ContentTypeControllerBase<TContentType> : BackOfficeNotificationsController
    where TContentType : class, IContentTypeComposition
{
    private readonly EditorValidatorCollection _editorValidatorCollection;

    protected ContentTypeControllerBase(
        ICultureDictionary cultureDictionary,
        EditorValidatorCollection editorValidatorCollection,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IUmbracoMapper umbracoMapper,
        ILocalizedTextService localizedTextService)
    {
        _editorValidatorCollection = editorValidatorCollection ??
                                     throw new ArgumentNullException(nameof(editorValidatorCollection));
        CultureDictionary = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
        ContentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        MediaTypeService = mediaTypeService ?? throw new ArgumentNullException(nameof(mediaTypeService));
        MemberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        UmbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        LocalizedTextService =
            localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
    }

    protected ICultureDictionary CultureDictionary { get; }
    public IContentTypeService ContentTypeService { get; }
    public IMediaTypeService MediaTypeService { get; }
    public IMemberTypeService MemberTypeService { get; }
    public IUmbracoMapper UmbracoMapper { get; }
    public ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    ///     Returns the available composite content types for a given content type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filterContentTypes">
    ///     This is normally an empty list but if additional content type aliases are passed in, any content types containing
    ///     those aliases will be filtered out
    ///     along with any content types that have matching property types that are included in the filtered content types
    /// </param>
    /// <param name="filterPropertyTypes">
    ///     This is normally an empty list but if additional property type aliases are passed in, any content types that have
    ///     these aliases will be filtered out.
    ///     This is required because in the case of creating/modifying a content type because new property types being added to
    ///     it are not yet persisted so cannot
    ///     be looked up via the db, they need to be passed in.
    /// </param>
    /// <param name="contentTypeId"></param>
    /// <param name="isElement">Whether the composite content types should be applicable for an element type</param>
    /// <returns></returns>
    protected ActionResult<IEnumerable<Tuple<EntityBasic?, bool>>> PerformGetAvailableCompositeContentTypes(
        int contentTypeId,
        UmbracoObjectTypes type,
        string[]? filterContentTypes,
        string[]? filterPropertyTypes,
        bool isElement)
    {
        IContentTypeComposition? source = null;

        //below is all ported from the old doc type editor and comes with the same weaknesses /insanity / magic

        IContentTypeComposition[] allContentTypes;

        switch (type)
        {
            case UmbracoObjectTypes.DocumentType:
                if (contentTypeId > 0)
                {
                    source = ContentTypeService.Get(contentTypeId);
                    if (source == null)
                    {
                        return NotFound();
                    }
                }

                allContentTypes = ContentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
                break;

            case UmbracoObjectTypes.MediaType:
                if (contentTypeId > 0)
                {
                    source = MediaTypeService.Get(contentTypeId);
                    if (source == null)
                    {
                        return NotFound();
                    }
                }

                allContentTypes = MediaTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
                break;

            case UmbracoObjectTypes.MemberType:
                if (contentTypeId > 0)
                {
                    source = MemberTypeService.Get(contentTypeId);
                    if (source == null)
                    {
                        return NotFound();
                    }
                }

                allContentTypes = MemberTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
                break;

            default:
                throw new ArgumentOutOfRangeException("The entity type was not a content type");
        }

        ContentTypeAvailableCompositionsResults availableCompositions =
            ContentTypeService.GetAvailableCompositeContentTypes(source, allContentTypes, filterContentTypes, filterPropertyTypes, isElement);


        IContentTypeComposition[] currCompositions =
            source == null ? new IContentTypeComposition[] { } : source.ContentTypeComposition.ToArray();
        var compAliases = currCompositions.Select(x => x.Alias).ToArray();
        IEnumerable<string> ancestors = availableCompositions.Ancestors.Select(x => x.Alias);

        return availableCompositions.Results
            .Select(x =>
                new Tuple<EntityBasic?, bool>(UmbracoMapper.Map<IContentTypeComposition, EntityBasic>(x.Composition), x.Allowed))
            .Select(x =>
            {
                //we need to ensure that the item is enabled if it is already selected
                // but do not allow it if it is any of the ancestors
                if (compAliases.Contains(x.Item1?.Alias) && ancestors.Contains(x.Item1?.Alias) == false)
                {
                    //re-set x to be allowed (NOTE: I didn't know you could set an enumerable item in a lambda!)
                    x = new Tuple<EntityBasic?, bool>(x.Item1, true);
                }

                //translate the name
                if (x.Item1 is not null)
                {
                    x.Item1.Name = TranslateItem(x.Item1.Name);
                }

                IContentTypeComposition? contentType = allContentTypes.FirstOrDefault(c => c.Key == x.Item1?.Key);
                EntityContainer[] containers = GetEntityContainers(contentType, type).ToArray();
                var containerPath =
                    $"/{(containers.Any() ? $"{string.Join("/", containers.Select(c => c.Name))}/" : null)}";
                if (x.Item1 is not null)
                {
                    x.Item1.AdditionalData["containerPath"] = containerPath;
                }

                return x;
            })
            .ToList();
    }

    private IEnumerable<EntityContainer> GetEntityContainers(IContentTypeComposition? contentType, UmbracoObjectTypes type)
    {
        if (contentType == null)
        {
            return Enumerable.Empty<EntityContainer>();
        }

        switch (type)
        {
            case UmbracoObjectTypes.DocumentType:
                if (contentType is IContentType documentContentType)
                {
                    return ContentTypeService.GetContainers(documentContentType);
                }

                return Enumerable.Empty<EntityContainer>();
            case UmbracoObjectTypes.MediaType:
                if (contentType is IMediaType mediaContentType)
                {
                    return MediaTypeService.GetContainers(mediaContentType);
                }

                return Enumerable.Empty<EntityContainer>();
            case UmbracoObjectTypes.MemberType:
                return Enumerable.Empty<EntityContainer>();
            default:
                throw new ArgumentOutOfRangeException("The entity type was not a content type");
        }
    }

    /// <summary>
    ///     Returns a list of content types where a particular composition content type is used
    /// </summary>
    /// <param name="type">Type of content Type, eg documentType or mediaType</param>
    /// <param name="contentTypeId">Id of composition content type</param>
    /// <returns></returns>
    protected ActionResult<IEnumerable<EntityBasic>> PerformGetWhereCompositionIsUsedInContentTypes(
        int contentTypeId, UmbracoObjectTypes type)
    {
        var id = 0;

        if (contentTypeId > 0)
        {
            IContentTypeComposition? source;

            switch (type)
            {
                case UmbracoObjectTypes.DocumentType:
                    source = ContentTypeService.Get(contentTypeId);
                    break;

                case UmbracoObjectTypes.MediaType:
                    source = MediaTypeService.Get(contentTypeId);
                    break;

                case UmbracoObjectTypes.MemberType:
                    source = MemberTypeService.Get(contentTypeId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (source == null)
            {
                return NotFound();
            }

            id = source.Id;
        }

        IEnumerable<IContentTypeComposition> composedOf;

        switch (type)
        {
            case UmbracoObjectTypes.DocumentType:
                composedOf = ContentTypeService.GetComposedOf(id);
                break;

            case UmbracoObjectTypes.MediaType:
                composedOf = MediaTypeService.GetComposedOf(id);
                break;

            case UmbracoObjectTypes.MemberType:
                composedOf = MemberTypeService.GetComposedOf(id);
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
            .Select(UmbracoMapper.Map<IContentTypeComposition, EntityBasic>)
            .WhereNotNull()
            .Select(TranslateName)
            .ToList();
    }

    protected string? TranslateItem(string? text)
    {
        if (text == null)
        {
            return null;
        }

        if (text.StartsWith("#") == false)
        {
            return text;
        }

        text = text.Substring(1);
        return CultureDictionary[text].IfNullOrWhiteSpace(text);
    }

    protected ActionResult<TContentType?> PerformPostSave<TContentTypeDisplay, TContentTypeSave, TPropertyType>(
        TContentTypeSave contentTypeSave,
        Func<int, TContentType?> getContentType,
        Action<TContentType?> saveContentType,
        Action<TContentTypeSave>? beforeCreateNew = null)
        where TContentTypeDisplay : ContentTypeCompositionDisplay
        where TContentTypeSave : ContentTypeSave<TPropertyType>
        where TPropertyType : PropertyTypeBasic
    {
        var ctId = Convert.ToInt32(contentTypeSave.Id);
        TContentType? ct = ctId > 0 ? getContentType(ctId) : null;
        if (ctId > 0 && ct == null)
        {
            return NotFound();
        }

        //Validate that there's no other ct with the same alias
        // it in fact cannot be the same as any content type alias (member, content or media) because
        // this would interfere with how ModelsBuilder works and also how many of the published caches
        // works since that is based on aliases.
        IEnumerable<string> allAliases = ContentTypeService.GetAllContentTypeAliases();
        var exists = allAliases.InvariantContains(contentTypeSave.Alias);
        if (exists && (ctId == 0 || (!ct?.Alias.InvariantEquals(contentTypeSave.Alias) ?? false)))
        {
            ModelState.AddModelError("Alias", LocalizedTextService.Localize("editcontenttype", "aliasAlreadyExists"));
        }

        // execute the external validators
        ValidateExternalValidators(ModelState, contentTypeSave);

        if (ModelState.IsValid == false)
        {
            TContentTypeDisplay? err =
                CreateModelStateValidationEror<TContentTypeSave, TContentTypeDisplay>(ctId, contentTypeSave, ct);
            return ValidationProblem(err);
        }

        //filter out empty properties
        contentTypeSave.Groups = contentTypeSave.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
        foreach (PropertyGroupBasic<TPropertyType> group in contentTypeSave.Groups)
        {
            group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
        }

        if (ctId > 0)
        {
            //its an update to an existing content type

            //This mapping will cause a lot of content type validation to occur which we need to deal with
            try
            {
                UmbracoMapper.Map(contentTypeSave, ct);
            }
            catch (Exception ex)
            {
                TContentTypeDisplay? responseEx =
                    CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave, TPropertyType>(
                        ex, contentTypeSave, ct, ctId);
                if (responseEx != null)
                {
                    return ValidationProblem(responseEx);
                }
            }

            TContentTypeDisplay? exResult =
                CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(
                    contentTypeSave, ct);
            if (exResult != null)
            {
                return ValidationProblem(exResult);
            }

            saveContentType(ct);

            return ct;
        }
        else
        {
            beforeCreateNew?.Invoke(contentTypeSave);

            //check if the type is trying to allow type 0 below itself - id zero refers to the currently unsaved type
            //always filter these 0 types out
            var allowItselfAsChild = false;
            var allowIfselfAsChildSortOrder = -1;
            if (contentTypeSave.AllowedContentTypes != null)
            {
                allowIfselfAsChildSortOrder = contentTypeSave.AllowedContentTypes.IndexOf(0);
                allowItselfAsChild = contentTypeSave.AllowedContentTypes.Any(x => x == 0);

                contentTypeSave.AllowedContentTypes =
                    contentTypeSave.AllowedContentTypes.Where(x => x > 0).ToList();
            }

            //save as new

            TContentType? newCt = null;
            try
            {
                //This mapping will cause a lot of content type validation to occur which we need to deal with
                newCt = UmbracoMapper.Map<TContentType>(contentTypeSave);
            }
            catch (Exception ex)
            {
                TContentTypeDisplay? responseEx =
                    CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave, TPropertyType>(
                        ex, contentTypeSave, ct, ctId);
                if (responseEx is null)
                {
                    throw;
                }

                return ValidationProblem(responseEx);
            }

            TContentTypeDisplay? exResult =
                CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType, TContentTypeDisplay>(
                    contentTypeSave, newCt);
            if (exResult != null)
            {
                return ValidationProblem(exResult);
            }

            //set id to null to ensure its handled as a new type
            contentTypeSave.Id = null;
            contentTypeSave.CreateDate = DateTime.Now;
            contentTypeSave.UpdateDate = DateTime.Now;

            saveContentType(newCt);

            //we need to save it twice to allow itself under itself.
            if (allowItselfAsChild && newCt != null)
            {
                newCt.AllowedContentTypes =
                    newCt.AllowedContentTypes?.Union(
                        new[] { new ContentTypeSort(newCt.Id, allowIfselfAsChildSortOrder) });
                saveContentType(newCt);
            }

            return newCt;
        }
    }

    private void ValidateExternalValidators(ModelStateDictionary modelState, object model)
    {
        Type modelType = model.GetType();

        IEnumerable<ValidationResult> validationResults = _editorValidatorCollection
            .Where(x => x.ModelType == modelType)
            .SelectMany(x => x.Validate(model))
            .Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage) && x.MemberNames.Any());

        foreach (ValidationResult r in validationResults)
        {
            foreach (var m in r.MemberNames)
            {
                modelState.AddModelError(m, r.ErrorMessage ?? string.Empty);
            }
        }
    }

    /// <summary>
    ///     Move
    /// </summary>
    /// <param name="move"></param>
    /// <param name="getContentType"></param>
    /// <param name="doMove"></param>
    /// <returns></returns>
    protected IActionResult PerformMove(
        MoveOrCopy move,
        Func<int, TContentType?> getContentType,
        Func<TContentType, int, Attempt<OperationResult<MoveOperationStatusType>?>> doMove)
    {
        TContentType? toMove = getContentType(move.Id);
        if (toMove == null)
        {
            return NotFound();
        }

        Attempt<OperationResult<MoveOperationStatusType>?> result = doMove(toMove, move.ParentId);
        if (result.Success)
        {
            return Content(toMove.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        switch (result.Result?.Result)
        {
            case MoveOperationStatusType.FailedParentNotFound:
                return NotFound();
            case MoveOperationStatusType.FailedCancelledByEvent:
                return ValidationProblem();
            case MoveOperationStatusType.FailedNotAllowedByPath:
                return ValidationProblem(LocalizedTextService.Localize("moveOrCopy", "notAllowedByPath"));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Move
    /// </summary>
    /// <param name="move"></param>
    /// <param name="getContentType"></param>
    /// <param name="doCopy"></param>
    /// <returns></returns>
    protected IActionResult PerformCopy(
        MoveOrCopy move,
        Func<int, TContentType?> getContentType,
        Func<TContentType, int, Attempt<OperationResult<MoveOperationStatusType, TContentType>?>> doCopy)
    {
        TContentType? toMove = getContentType(move.Id);
        if (toMove == null)
        {
            return NotFound();
        }

        Attempt<OperationResult<MoveOperationStatusType, TContentType>?> result = doCopy(toMove, move.ParentId);
        if (result.Success)
        {
            return Content(toMove.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        switch (result.Result?.Result)
        {
            case MoveOperationStatusType.FailedParentNotFound:
                return NotFound();
            case MoveOperationStatusType.FailedCancelledByEvent:
                return ValidationProblem();
            case MoveOperationStatusType.FailedNotAllowedByPath:
                return ValidationProblem(LocalizedTextService.Localize("moveOrCopy", "notAllowedByPath"));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Validates the composition and adds errors to the model state if any are found then throws an error response if
    ///     there are errors
    /// </summary>
    /// <param name="contentTypeSave"></param>
    /// <param name="composition"></param>
    /// <returns></returns>
    private TContentTypeDisplay? CreateCompositionValidationExceptionIfInvalid<TContentTypeSave, TPropertyType,
        TContentTypeDisplay>(TContentTypeSave contentTypeSave, TContentType? composition)
        where TContentTypeSave : ContentTypeSave<TPropertyType>
        where TPropertyType : PropertyTypeBasic
        where TContentTypeDisplay : ContentTypeCompositionDisplay
    {
        IContentTypeBaseService<TContentType>? service = GetContentTypeService<TContentType>();
        Attempt<string[]?> validateAttempt = service?.ValidateComposition(composition) ?? Attempt.Fail<string[]?>();
        if (validateAttempt == false)
        {
            // if it's not successful then we need to return some model state for the property type and property group
            // aliases that are duplicated
            IEnumerable<string>? duplicatePropertyTypeAliases = validateAttempt.Result?.Distinct();
            var invalidPropertyGroupAliases =
                (validateAttempt.Exception as InvalidCompositionException)?.PropertyGroupAliases ??
                Array.Empty<string>();

            AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(contentTypeSave, duplicatePropertyTypeAliases, invalidPropertyGroupAliases);

            TContentTypeDisplay? display = UmbracoMapper.Map<TContentTypeDisplay>(composition);
            //map the 'save' data on top
            display = UmbracoMapper.Map(contentTypeSave, display);
            if (display is not null)
            {
                display.Errors = ModelState.ToErrorDictionary();
            }

            return display;
        }

        return null;
    }

    public IContentTypeBaseService<T>? GetContentTypeService<T>()
        where T : IContentTypeComposition
    {
        if (typeof(T).Implements<IContentType>())
        {
            return ContentTypeService as IContentTypeBaseService<T>;
        }

        if (typeof(T).Implements<IMediaType>())
        {
            return MediaTypeService as IContentTypeBaseService<T>;
        }

        if (typeof(T).Implements<IMemberType>())
        {
            return MemberTypeService as IContentTypeBaseService<T>;
        }

        throw new ArgumentException("Type " + typeof(T).FullName + " does not have a service.");
    }

    /// <summary>
    ///     Adds errors to the model state if any invalid aliases are found then throws an error response if there are errors
    /// </summary>
    /// <param name="contentTypeSave"></param>
    /// <param name="duplicatePropertyTypeAliases"></param>
    /// <param name="invalidPropertyGroupAliases"></param>
    /// <returns></returns>
    private void AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(
        TContentTypeSave contentTypeSave,
        IEnumerable<string>? duplicatePropertyTypeAliases,
        IEnumerable<string>? invalidPropertyGroupAliases)
        where TContentTypeSave : ContentTypeSave<TPropertyType>
        where TPropertyType : PropertyTypeBasic
    {
        if (duplicatePropertyTypeAliases is not null)
        {
            foreach (var propertyTypeAlias in duplicatePropertyTypeAliases)
            {
                // Find the property type relating to these
                TPropertyType property = contentTypeSave.Groups.SelectMany(x => x.Properties)
                    .Single(x => x.Alias == propertyTypeAlias);
                PropertyGroupBasic<TPropertyType> group =
                    contentTypeSave.Groups.Single(x => x.Properties.Contains(property));
                var propertyIndex = group.Properties.IndexOf(property);
                var groupIndex = contentTypeSave.Groups.IndexOf(group);

                var key = $"Groups[{groupIndex}].Properties[{propertyIndex}].Alias";
                ModelState.AddModelError(key, "Duplicate property aliases aren't allowed between compositions");
            }
        }

        if (invalidPropertyGroupAliases is not null)
        {
            foreach (var propertyGroupAlias in invalidPropertyGroupAliases)
            {
                // Find the property group relating to these
                PropertyGroupBasic<TPropertyType> group =
                    contentTypeSave.Groups.Single(x => x.Alias == propertyGroupAlias);
                var groupIndex = contentTypeSave.Groups.IndexOf(group);
                var key = $"Groups[{groupIndex}].Name";
                ModelState.AddModelError(key, "Different group types aren't allowed between compositions");
            }
        }
    }

    /// <summary>
    ///     If the exception is an InvalidCompositionException create a response exception to be thrown for validation errors
    /// </summary>
    /// <typeparam name="TContentTypeDisplay"></typeparam>
    /// <typeparam name="TContentTypeSave"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="ex"></param>
    /// <param name="contentTypeSave"></param>
    /// <param name="ct"></param>
    /// <param name="ctId"></param>
    /// <returns></returns>
    private TContentTypeDisplay? CreateInvalidCompositionResponseException<TContentTypeDisplay, TContentTypeSave,
        TPropertyType>(
        Exception ex, TContentTypeSave contentTypeSave, TContentType? ct, int ctId)
        where TContentTypeDisplay : ContentTypeCompositionDisplay
        where TContentTypeSave : ContentTypeSave<TPropertyType>
        where TPropertyType : PropertyTypeBasic
    {
        InvalidCompositionException? invalidCompositionException = null;
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
            AddCompositionValidationErrors<TContentTypeSave, TPropertyType>(
                contentTypeSave,
                invalidCompositionException.PropertyTypeAliases,
                invalidCompositionException.PropertyGroupAliases);
            return CreateModelStateValidationEror<TContentTypeSave, TContentTypeDisplay>(ctId, contentTypeSave, ct);
        }

        return null;
    }

    /// <summary>
    ///     Used to throw the ModelState validation results when the ModelState is invalid
    /// </summary>
    /// <typeparam name="TContentTypeDisplay"></typeparam>
    /// <typeparam name="TContentTypeSave"></typeparam>
    /// <param name="ctId"></param>
    /// <param name="contentTypeSave"></param>
    /// <param name="ct"></param>
    private TContentTypeDisplay? CreateModelStateValidationEror<TContentTypeSave, TContentTypeDisplay>(int ctId, TContentTypeSave contentTypeSave, TContentType? ct)
        where TContentTypeDisplay : ContentTypeCompositionDisplay
        where TContentTypeSave : ContentTypeSave
    {
        TContentTypeDisplay? forDisplay;
        if (ctId > 0)
        {
            //Required data is invalid so we cannot continue
            forDisplay = UmbracoMapper.Map<TContentTypeDisplay>(ct);
            //map the 'save' data on top
            forDisplay = UmbracoMapper.Map(contentTypeSave, forDisplay);
        }
        else
        {
            //map the 'save' data to display
            forDisplay = UmbracoMapper.Map<TContentTypeDisplay>(contentTypeSave);
        }

        if (forDisplay is not null)
        {
            forDisplay.Errors = ModelState.ToErrorDictionary();
        }

        return forDisplay;
    }
}
