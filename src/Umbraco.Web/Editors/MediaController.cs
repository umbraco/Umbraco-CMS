using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{

    //internal interface IUmbracoApiService<T>
    //{
    //    T Get(int id);
    //    IEnumerable<T> GetChildren(int id);
    //    HttpResponseMessage Delete(int id);
    //    //copy
    //    //move
    //    //update
    //    //create
    //}

    [PluginController("UmbracoApi")]
    public class MediaController : UmbracoAuthorizedApiController
    {
        private readonly MediaModelMapper _mediaModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaController()
            : this(UmbracoContext.Current, new MediaModelMapper(UmbracoContext.Current.Application, new UserModelMapper()))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="mediaModelMapper"></param>
        internal MediaController(UmbracoContext umbracoContext, MediaModelMapper mediaModelMapper)
            : base(umbracoContext)
        {
            _mediaModelMapper = mediaModelMapper;
        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public MediaItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetMediaType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = new Umbraco.Core.Models.Media("Empty", parentId, contentType);
            return _mediaModelMapper.ToMediaItemDisplay(emptyContent);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MediaItemDisplay GetById(int id)
        {
            var foundContent = Services.MediaService.GetById(id);
            if (foundContent == null)
            {
                ModelState.AddModelError("id", string.Format("media with id: {0} was not found", id));
                var errorResponse = Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    ModelState);
                throw new HttpResponseException(errorResponse);
            }
            return _mediaModelMapper.ToMediaItemDisplay(foundContent);
        }

        /// <summary>
        /// Returns the root media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetRootMedia()
        {
            return Services.MediaService.GetRootMedia()
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }

        /// <summary>
        /// Returns the child media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(int parentId)
        {
            return Services.MediaService.GetChildren(parentId)
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [ContentItemValidationFilter(typeof(ContentItemValidationHelper<IMedia>))]
        [FileUploadCleanupFilter]
        public MediaItemDisplay PostSave(
            [ModelBinder(typeof(MediaItemBinder))]
                ContentItemSave<IMedia> contentItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object

            //Now, we just need to save the data

            contentItem.PersistedContent.Name = contentItem.Name;
            //TODO: We'll need to save the new template, publishat, etc... values here

            //Save the property values (for properties that have a valid editor ... not legacy)
            foreach (var p in contentItem.ContentDto.Properties.Where(x => x.PropertyEditor != null))
            {
                //get the dbo property
                var dboProperty = contentItem.PersistedContent.Properties[p.Alias];

                //create the property data to send to the property editor
                var d = new Dictionary<string, object>();
                //add the files if any
                var files = contentItem.UploadedFiles.Where(x => x.PropertyId == p.Id).ToArray();
                if (files.Any())
                {
                    d.Add("files", files);
                }
                var data = new ContentPropertyData(p.Value, d);

                //get the deserialized value from the property editor
                if (p.PropertyEditor == null)
                {
                    LogHelper.Warn<MediaController>("No property editor found for property " + p.Alias);
                }
                else
                {
                    dboProperty.Value = p.PropertyEditor.ValueEditor.DeserializeValue(data, dboProperty.Value);
                }
            }

            //save the item
            Services.MediaService.Save(contentItem.PersistedContent);

            //return the updated model
            return _mediaModelMapper.ToMediaItemDisplay(contentItem.PersistedContent);
        }
    }
}
