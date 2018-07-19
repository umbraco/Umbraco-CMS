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

namespace Umbraco.Web.Editors.Binders
{
    /// <inheritdoc />
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.MediaItemSave" />
    /// </summary>
    internal class MediaItemBinder : ContentItemBaseBinder<IMedia, MediaItemSave>
    {
        public MediaItemBinder() : this(Current.Logger, Current.Services, Current.UmbracoContextAccessor)
        {
        }

        public MediaItemBinder(Core.Logging.ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, services, umbracoContextAccessor)
        {
        }

        /// <summary>
        /// Overridden to trim the name
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var result = base.BindModel(actionContext, bindingContext);
            if (result)
            {
                var model = (MediaItemSave) bindingContext.Model;
                model.Name = model.Name.Trim();
            }
            return result;
        }

        protected override IMedia GetExisting(MediaItemSave model)
        {
            return Services.MediaService.GetById(Convert.ToInt32(model.Id));
        }

        protected override IMedia CreateNew(MediaItemSave model)
        {
            var mediaType = Services.MediaTypeService.Get(model.ContentTypeAlias);
            if (mediaType == null)
            {
                throw new InvalidOperationException("No media type found with alias " + model.ContentTypeAlias);
            }
            return new Core.Models.Media(model.Name, model.ParentId, mediaType);
        }

        protected override ContentItemDto<IMedia> MapFromPersisted(MediaItemSave model)
        {
            return Mapper.Map<IMedia, ContentItemDto<IMedia>>(model.PersistedContent);
        }
    }
}
