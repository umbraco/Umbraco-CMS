using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Extensions;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.Editors.Binders
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
        // private readonly UmbracoMapper _umbracoMapper;
        // private readonly ContentTypeService _contentTypeService;
        // private readonly ContentService _contentService;
        // private readonly ContentModelBinderHelper _modelBinderHelper;
        //
        // public ContentItemBinder(UmbracoMapper umbracoMapper, ContentTypeService contentTypeService, ContentService contentService)
        // {
        //     _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        //     _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        //     _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        //     _modelBinderHelper = new ContentModelBinderHelper();
        // }
        //
        // /// <summary>
        // /// Creates the model from the request and binds it to the context
        // /// </summary>
        // /// <param name="actionContext"></param>
        // /// <param name="bindingContext"></param>
        // /// <returns></returns>
        // public Task BindModelAsync(ModelBindingContext bindingContext)
        // {
        //     var actionContext = bindingContext.ActionContext;
        //
        //     var model = _modelBinderHelper.BindModelFromMultipartRequest<ContentItemSave>(actionContext, bindingContext);
        //     if (model == null)
        //     {
        //         bindingContext.Result = ModelBindingResult.Failed();
        //         return Task.CompletedTask;
        //     }
        //
        //     model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);
        //
        //     //create the dto from the persisted model
        //     if (model.PersistedContent != null)
        //     {
        //         foreach (var variant in model.Variants)
        //         {
        //             //map the property dto collection with the culture of the current variant
        //             variant.PropertyCollectionDto = _umbracoMapper.Map<ContentPropertyCollectionDto>(
        //                 model.PersistedContent,
        //                 context =>
        //                 {
        //                     // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
        //                     context.SetCulture(variant.Culture);
        //                     context.SetSegment(variant.Segment);
        //                 });
        //
        //             //now map all of the saved values to the dto
        //             _modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
        //         }
        //     }
        //
        //     return Task.CompletedTask;
        // }
        //
        // public bool BindModel(ActionContext actionContext, ModelBindingContext bindingContext)
        // {
        //     var model = _modelBinderHelper.BindModelFromMultipartRequest<ContentItemSave>(actionContext, bindingContext);
        //     if (model == null) return false;
        //
        //     model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);
        //
        //     //create the dto from the persisted model
        //     if (model.PersistedContent != null)
        //     {
        //         foreach (var variant in model.Variants)
        //         {
        //             //map the property dto collection with the culture of the current variant
        //             variant.PropertyCollectionDto = _umbracoMapper.Map<ContentPropertyCollectionDto>(
        //                 model.PersistedContent,
        //                 context =>
        //                 {
        //                     // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
        //                     context.SetCulture(variant.Culture);
        //                     context.SetSegment(variant.Segment);
        //                 });
        //
        //             //now map all of the saved values to the dto
        //             _modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
        //         }
        //     }
        //
        //     return true;
        // }
        //
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
            var modelName = bindingContext.ModelName;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return;
            }
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            var model = _jsonSerializer.Deserialize<ContentItemSave>(value);
            if (model is null)
            {
                // Non-integer arguments result in model state errors
                bindingContext.ModelState.TryAddModelError(
                    modelName, $"Cannot deserialize {modelName} as {nameof(ContentItemSave)}.");

                return;
            }

            //Handle file uploads
            foreach (var formFile in bindingContext.HttpContext.Request.Form.Files)
            {
                   //The name that has been assigned in JS has 2 or more parts. The second part indicates the property id
                // for which the file belongs, the remaining parts are just metadata that can be used by the property editor.
                var parts = formFile.Name.Trim('\"').Split('_');
                if (parts.Length < 2)
                {
                    bindingContext.HttpContext.SetReasonPhrase( "The request was not formatted correctly the file name's must be underscore delimited");
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
                var propAlias = parts[1];

                //if there are 3 parts part 3 is always culture
                string culture = null;
                if (parts.Length > 2)
                {
                    culture = parts[2];
                    //normalize to null if empty
                    if (culture.IsNullOrWhiteSpace())
                    {
                        culture = null;
                    }
                }

                //if there are 4 parts part 4 is always segment
                string segment = null;
                if (parts.Length > 3)
                {
                    segment = parts[3];
                    //normalize to null if empty
                    if (segment.IsNullOrWhiteSpace())
                    {
                        segment = null;
                    }
                }

                // TODO: anything after 4 parts we can put in metadata

                var fileName = formFile.FileName.Trim('\"');

                var tempFileUploadFolder = _hostingEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.TempFileUploads);
                Directory.CreateDirectory(tempFileUploadFolder);
                var tempFilePath = Path.Combine(tempFileUploadFolder, Guid.NewGuid().ToString());

                using (var stream = System.IO.File.Create(tempFilePath))
                {
                    await formFile.CopyToAsync(stream);
                }

                model.UploadedFiles.Add(new ContentPropertyFile
                {
                    TempFilePath = tempFilePath,
                    PropertyAlias = propAlias,
                    Culture = culture,
                    Segment = segment,
                    FileName = fileName
                });
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
