using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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
    
    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]    
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

        public MemberTypeDisplay GetById(int id)
        {
            var ct = Services.MemberTypeService.Get(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMemberType, MemberTypeDisplay>(ct);
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

        /// <summary>
        /// Returns the avilable compositions for this content type
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="filterContentTypes">
        /// This is normally an empty list but if additional content type aliases are passed in, any content types containing those aliases will be filtered out
        /// along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filterPropertyTypes">
        /// This is normally an empty list but if additional property type aliases are passed in, any content types that have these aliases will be filtered out.
        /// This is required because in the case of creating/modifying a content type because new property types being added to it are not yet persisted so cannot
        /// be looked up via the db, they need to be passed in.
        /// </param>
        /// <returns></returns>
        public HttpResponseMessage GetAvailableCompositeMemberTypes(int contentTypeId,
            [FromUri]string[] filterContentTypes,
            [FromUri]string[] filterPropertyTypes)
        {
            var result = PerformGetAvailableCompositeContentTypes(contentTypeId, UmbracoObjectTypes.MemberType, filterContentTypes, filterPropertyTypes)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Request.CreateResponse(result);
        }

        public MemberTypeDisplay GetEmpty()
        {
            var ct = new MemberType(-1);
            ct.Icon = "icon-user";

            var dto = Mapper.Map<IMemberType, MemberTypeDisplay>(ct);
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

        public MemberTypeDisplay PostSave(MemberTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<IMemberType, MemberTypeDisplay, MemberTypeSave, MemberPropertyTypeBasic>(
                contentTypeSave:            contentTypeSave,
                getContentType:             i => Services.MemberTypeService.Get(i),
                saveContentType:            type => Services.MemberTypeService.Save(type));

            var display = Mapper.Map<MemberTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/memberTypeSavedHeader"),
                            string.Empty);

            return display;
        }
    }
}