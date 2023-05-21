using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelBinders;

/// <summary>
///     The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.ContentItemSave" />
/// </summary>
internal class ContentItemBinder : IModelBinder
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ContentModelBinderHelper _modelBinderHelper;

    public ContentItemBinder(
        IJsonSerializer jsonSerializer,
        IUmbracoMapper umbracoMapper,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IHostingEnvironment hostingEnvironment)
    {
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _modelBinderHelper = new ContentModelBinderHelper();
    }


    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        ContentItemSave? model =
            await _modelBinderHelper.BindModelFromMultipartRequestAsync<ContentItemSave>(_jsonSerializer,
                _hostingEnvironment, bindingContext);

        if (model is null)
        {
            return;
        }

        IContent? persistedContent =
            ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);
        BindModel(model, persistedContent!, _modelBinderHelper, _umbracoMapper);

        bindingContext.Result = ModelBindingResult.Success(model);
    }

    protected virtual IContent? GetExisting(ContentItemSave model) => _contentService.GetById(model.Id);

    private IContent CreateNew(ContentItemSave model)
    {
        IContentType? contentType = _contentTypeService.Get(model.ContentTypeAlias);
        if (contentType == null)
        {
            throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
        }

        return new Content(
            contentType.VariesByCulture() ? null : model.Variants.First().Name,
            model.ParentId,
            contentType);
    }

    internal static void BindModel(ContentItemSave model, IContent persistedContent,
        ContentModelBinderHelper modelBinderHelper, IUmbracoMapper umbracoMapper)
    {
        model.PersistedContent = persistedContent;

        //create the dto from the persisted model
        if (model.PersistedContent != null)
        {
            foreach (ContentVariantSave variant in model.Variants)
            {
                //map the property dto collection with the culture of the current variant
                variant.PropertyCollectionDto = umbracoMapper.Map<ContentPropertyCollectionDto>(
                    model.PersistedContent,
                    context =>
                    {
                        // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
                        context.SetCulture(variant.Culture);
                        context.SetSegment(variant.Segment);
                    });

                //now map all of the saved values to the dto
                modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
            }
        }
    }
}
