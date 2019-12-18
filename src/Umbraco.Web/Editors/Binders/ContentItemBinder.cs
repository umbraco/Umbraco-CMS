using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.ContentItemSave" />
    /// </summary>
    internal class ContentItemBinder : IModelBinder
    {
        private readonly ContentModelBinderHelper _modelBinderHelper;

        public ContentItemBinder() : this(Current.Logger, Current.Services, Current.UmbracoContextAccessor)
        {
        }

        public ContentItemBinder(ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor)
        {
            Services = services;
            _modelBinderHelper = new ContentModelBinderHelper();
        }

        protected ServiceContext Services { get; }

        /// <summary>
        /// Creates the model from the request and binds it to the context
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var model = _modelBinderHelper.BindModelFromMultipartRequest<ContentItemSave>(actionContext, bindingContext);
            if (model == null) return false;

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                foreach (var variant in model.Variants)
                {
                    if (variant.Culture.IsNullOrWhiteSpace())
                    {
                        //map the property dto collection (no culture is passed to the mapping context so it will be invariant)
                        variant.PropertyCollectionDto = Current.Mapper.Map<ContentPropertyCollectionDto>(model.PersistedContent);
                    }
                    else
                    {
                        //map the property dto collection with the culture of the current variant
                        variant.PropertyCollectionDto = Current.Mapper.Map<ContentPropertyCollectionDto>(
                            model.PersistedContent,
                            context => context.SetCulture(variant.Culture));
                    }

                    //now map all of the saved values to the dto
                    _modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
                }
            }

            return true;
        }

        protected virtual IContent GetExisting(ContentItemSave model)
        {
            return Services.ContentService.GetById(model.Id);
        }

        private IContent CreateNew(ContentItemSave model)
        {
            var contentType = Services.ContentTypeService.Get(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
            }
            return new Content(
                contentType.VariesByCulture() ? null : model.Variants.First().Name,
                model.ParentId,
                contentType);
        }
        
    }
}
