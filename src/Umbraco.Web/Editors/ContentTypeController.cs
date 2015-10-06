using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;
using System;
using System.Net.Http;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
    [EnableOverrideAuthorization]
    public class ContentTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }


        public ContentTypeDisplay GetById(int id)
        {
            var ct = Services.ContentTypeService.GetContentType(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IContentType, ContentTypeDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Deletes a document type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.ContentTypeService.GetContentType(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.ContentTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateFolder(int parentId, string name)
        {
            var result = Services.ContentTypeService.CreateFolder(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id 
                : Request.CreateValidationErrorResponse(result.Exception.Message);
        }
        
        public ContentTypeDisplay PostSave(ContentTypeSave contentTypeSave)
        {
            var ctId = Convert.ToInt32(contentTypeSave.Id);

            var ctService = Services.ContentTypeService;

            if (ModelState.IsValid == false)
            {
                var ct = ctService.GetContentType(ctId);
                //Required data is invalid so we cannot continue
                var forDisplay = Mapper.Map<ContentTypeDisplay>(ct);
                //map the 'save' data on top
                forDisplay = Mapper.Map(contentTypeSave, forDisplay);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //TODO: Deal with validation for composition with property and group names/aliases
            
            //filter out empty properties
            contentTypeSave.Groups = contentTypeSave.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentTypeSave.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }

            //TODO: This all needs to be done in a transaction!!
            // Which means that all of this logic needs to take place inside the service

            if (ctId > 0)
            {
                //its an update to an existing
                var found = ctService.GetContentType(ctId);
                if (found == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                Mapper.Map(contentTypeSave, found);
                ctService.Save(found);
                
                return Mapper.Map<ContentTypeDisplay>(found);
            }
            else
            {
                //set id to null to ensure its handled as a new type
                contentTypeSave.Id = null;
                contentTypeSave.CreateDate = DateTime.Now;
                contentTypeSave.UpdateDate = DateTime.Now;
                
                //create a default template if it doesnt exist -but only if default template is == to the content type
                //TODO: Is this really what we want? What if we don't want any template assigned at all ?
                if (contentTypeSave.DefaultTemplate.IsNullOrWhiteSpace() == false && contentTypeSave.DefaultTemplate == contentTypeSave.Alias)
                {
                    var template = Services.FileService.GetTemplate(contentTypeSave.Alias);
                    if (template == null)
                    {
                        template = new Template(contentTypeSave.Name, contentTypeSave.Alias);
                        Services.FileService.SaveTemplate(template);
                    }

                    //make sure the template alias is set on the default and allowed template so we can map it back
                    contentTypeSave.DefaultTemplate = template.Alias;
                }

                //check if the type is trying to allow type 0 below itself - id zero refers to the currently unsaved type
                //always filter these 0 types out
                var allowItselfAsChild = false;
                if (contentTypeSave.AllowedContentTypes != null)
                {
                    allowItselfAsChild = contentTypeSave.AllowedContentTypes.Any(x => x == 0);
                    contentTypeSave.AllowedContentTypes = contentTypeSave.AllowedContentTypes.Where(x => x > 0).ToList();
                }

                //save as new
                var newCt = Mapper.Map<IContentType>(contentTypeSave);
                ctService.Save(newCt);

                //we need to save it twice to allow itself under itself.
                if (allowItselfAsChild)
                {
                    newCt.AddContentType(newCt);
                    ctService.Save(newCt);
                }
                
                return Mapper.Map<ContentTypeDisplay>(newCt);
            }
        }

        /// <summary>
        /// Returns an empty content type for use as a scaffold when creating a new type
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ContentTypeDisplay GetEmpty(int parentId)
        {
            var ct = new ContentType(parentId);
            ct.Icon = "icon-document";

            var dto = Mapper.Map<IContentType, ContentTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all content type objects
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {
            var types = Services.ContentTypeService.GetAllContentTypes();
            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>);

            return basics.Select(basic =>
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
                return basic;
            });
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes, Constants.Trees.Content)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IContentType> types;
            if (contentId == Constants.System.Root)
            {
                types = Services.ContentTypeService.GetAllContentTypes().ToList();

                //if no allowed root types are set, just return everything
                if (types.Any(x => x.AllowedAsRoot))
                    types = types.Where(x => x.AllowedAsRoot);
            }
            else
            {
                var contentItem = Services.ContentService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var ids = contentItem.ContentType.AllowedContentTypes.Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAllContentTypes(ids).ToList();
            }

            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>).ToList();

            foreach (var basic in basics)
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
            }

            return basics;
        }


    }
}