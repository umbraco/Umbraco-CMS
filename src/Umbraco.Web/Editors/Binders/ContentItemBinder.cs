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
        public ContentItemBinder() : this(Current.Services)
        {
        }

        public ContentItemBinder(ServiceContext services)
        {
            Services = services;
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
            var model = ContentModelBinderHelper.BindModelFromMultipartRequest<ContentItemSave>(actionContext, bindingContext);
            if (model == null) return false;

            BindModel(model, ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model));

            return true;
        }

        internal static void BindModel(ContentItemSave model, IContent persistedContent)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            model.PersistedContent = persistedContent;

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                foreach (var variant in model.Variants)
                {
                    //map the property dto collection with the culture of the current variant
                    variant.PropertyCollectionDto = Current.Mapper.Map<ContentPropertyCollectionDto>(
                        model.PersistedContent,
                        context =>
                        {
                            // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
                            context.SetCulture(variant.Culture);
                            context.SetSegment(variant.Segment);
                        });

                    //now map all of the saved values to the dto
                    ContentModelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
                }
            }
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
