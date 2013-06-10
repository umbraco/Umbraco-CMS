using System.Collections.Generic;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;

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
            : this(UmbracoContext.Current, new MediaModelMapper(UmbracoContext.Current.Application, new ProfileModelMapper()))
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
        /// Returns the root media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic>> GetRootMedia()
        {
            return Services.MediaService.GetRootMedia()
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }

        /// <summary>
        /// Returns the child media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic>> GetChildren(int parentId)
        {
            return Services.MediaService.GetChildren(parentId)
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }
    }
}
