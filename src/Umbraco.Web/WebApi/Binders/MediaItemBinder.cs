using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Binders
{
    internal class MediaItemBinder : ContentItemBaseBinder<IMedia, MediaItemSave>
    {        
        public MediaItemBinder(ApplicationContext applicationContext)
            : base(applicationContext)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaItemBinder()
            : this(ApplicationContext.Current)
        {
        }

        protected override IMedia GetExisting(MediaItemSave model)
        {
            return ApplicationContext.Services.MediaService.GetById(Convert.ToInt32(model.Id));
        }

        protected override IMedia CreateNew(MediaItemSave model)
        {
            var mediaType = ApplicationContext.Services.ContentTypeService.GetMediaType(model.ContentTypeAlias);
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