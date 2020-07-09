using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.BackOffice.ModelBinders
{
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.ContentItemSave" />
    /// </summary>
    internal class ContentItemBinder : IModelBinder
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private ContentModelBinderHelper _modelBinderHelper;

        public ContentItemBinder(
            IJsonSerializer jsonSerializer,
            UmbracoMapper umbracoMapper,
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

        protected virtual IContent GetExisting(ContentItemSave model)
        {
            return _contentService.GetById(model.Id);
        }

        private IContent CreateNew(ContentItemSave model)
        {
            var contentType = _contentTypeService.Get(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
            }
            return new Content(
                contentType.VariesByCulture() ? null : model.Variants.First().Name,
                model.ParentId,
                contentType);
        }


        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var model = await _modelBinderHelper.BindModelFromMultipartRequestAsync<ContentItemSave>(_jsonSerializer, _hostingEnvironment, bindingContext);

            if (model is null)
            {
                return;
            }

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                foreach (var variant in model.Variants)
                {
                    //map the property dto collection with the culture of the current variant
                    variant.PropertyCollectionDto = _umbracoMapper.Map<ContentPropertyCollectionDto>(
                        model.PersistedContent,
                        context =>
                        {
                            // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
                            context.SetCulture(variant.Culture);
                            context.SetSegment(variant.Segment);
                        });

                    //now map all of the saved values to the dto
                    _modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(model);
        }


    }
}
