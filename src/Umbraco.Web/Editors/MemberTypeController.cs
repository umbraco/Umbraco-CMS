using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Web.Http;
using System.Net;
using Umbraco.Core.PropertyEditors;
using System;
using System.Net.Http;
using ContentType = System.Net.Mime.ContentType;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
    [EnableOverrideAuthorization]
    public class MemberTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberTypeController()
            : this(UmbracoContext.Current)
        {
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MemberTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
        }

        private readonly MembershipProvider _provider;

        public ContentTypeCompositionDisplay GetById(int id)
        {
            var ct = Services.MemberTypeService.Get(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMemberType, ContentTypeCompositionDisplay>(ct);
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
            var foundType = Services.MemberTypeService.Get(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.MemberTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public ContentTypeCompositionDisplay GetEmpty()
        {
            var ct = new MemberType(-1);
            ct.Icon = "icon-user";

            var dto = Mapper.Map<IMemberType, ContentTypeCompositionDisplay>(ct);
            return dto;
        }

       
        /// <summary>
        /// Returns all member types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAllTypes()
        {
            if (_provider.IsUmbracoMembershipProvider())
            {
                return Services.MemberTypeService.GetAll()
                               .Select(Mapper.Map<IMemberType, ContentTypeBasic>);    
            }
            return Enumerable.Empty<ContentTypeBasic>();
        }

        public ContentTypeCompositionDisplay PostSave(ContentTypeCompositionDisplay contentType)
        {

            var ctService = ApplicationContext.Services.MemberTypeService;

            //TODO: warn on content type alias conflicts
            //TODO: warn on property alias conflicts

            //TODO: Validate the submitted model

            //filter out empty properties
            contentType.Groups = contentType.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentType.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }

            var ctId = Convert.ToInt32(contentType.Id);

            if (ctId > 0)
            {
                //its an update to an existing
                IMemberType found = ctService.Get(ctId);
                if (found == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                Mapper.Map(contentType, found);
                ctService.Save(found);

                //map the saved item back to the content type (it should now get id etc set)
                Mapper.Map(found, contentType);
                return contentType;
            }
            else
            {
                //ensure alias is set
                if (string.IsNullOrEmpty(contentType.Alias))
                    contentType.Alias = contentType.Name.ToSafeAlias();

                contentType.Id = null;

                //save as new
                IMemberType newCt = new MemberType(-1);
                Mapper.Map(contentType, newCt);

                ctService.Save(newCt);

                //map the saved item back to the content type (it should now get id etc set)
                Mapper.Map(newCt, contentType);
                return contentType;
            }

        }
    }
}