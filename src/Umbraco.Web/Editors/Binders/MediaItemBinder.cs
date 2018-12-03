using System;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.MediaItemSave" />
    /// </summary>
    internal class MediaItemBinder : IModelBinder
    {
        private readonly ContentModelBinderHelper _modelBinderHelper;
        private readonly ServiceContext _services;

        public MediaItemBinder() : this(Current.Services)
        {
        }

        public MediaItemBinder(ServiceContext services)
        {
            _services = services;
            _modelBinderHelper = new ContentModelBinderHelper();
        }

        /// <summary>
        /// Creates the model from the request and binds it to the context
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var model = _modelBinderHelper.BindModelFromMultipartRequest<MediaItemSave>(actionContext, bindingContext);
            if (model == null) return false;

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                model.PropertyCollectionDto = Mapper.Map<IMedia, ContentPropertyCollectionDto>(model.PersistedContent);
                //now map all of the saved values to the dto
                _modelBinderHelper.MapPropertyValuesFromSaved(model, model.PropertyCollectionDto);
            }

            model.Name = model.Name.Trim();

            return true;
        }

        private IMedia GetExisting(MediaItemSave model)
        {
            return _services.MediaService.GetById(Convert.ToInt32(model.Id));
        }

        private IMedia CreateNew(MediaItemSave model)
        {
            var mediaType = _services.MediaTypeService.Get(model.ContentTypeAlias);
            if (mediaType == null)
            {
                throw new InvalidOperationException("No media type found with alias " + model.ContentTypeAlias);
            }
            return new Core.Models.Media(model.Name, model.ParentId, mediaType);
        }

    }
}
