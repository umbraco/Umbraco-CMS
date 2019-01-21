using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// An API controller used for dealing with member types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(new string[] { Constants.Trees.MemberTypes, Constants.Trees.Members})]    
    public class MemberTypeController : ContentTypeControllerBase<IMemberType>
    {
        public MemberTypeController(ICultureDictionaryFactory cultureDictionaryFactory, IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState)
            : base(cultureDictionaryFactory, globalSettings, umbracoContext, sqlContext, services, appCaches, logger, runtimeState)
        {
        }

        private readonly MembershipProvider _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
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
        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
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

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
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

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
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

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        public MemberTypeDisplay PostSave(MemberTypeSave contentTypeSave)
        {
            //get the persisted member type
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            var ct = ctId > 0 ? Services.MemberTypeService.Get(ctId) : null;

            if (UmbracoContext.Security.CurrentUser.HasAccessToSensitiveData() == false)
            {
                //We need to validate if any properties on the contentTypeSave have had their IsSensitiveValue changed,
                //and if so, we need to check if the current user has access to sensitive values. If not, we have to return an error
                var props = contentTypeSave.Groups.SelectMany(x => x.Properties);
                if (ct != null)
                {
                    foreach (var prop in props)
                    {
                        // Id 0 means the property was just added, no need to look it up
                        if (prop.Id == 0)
                            continue;
                        
                        var foundOnContentType = ct.PropertyTypes.FirstOrDefault(x => x.Id == prop.Id);
                        if (foundOnContentType == null)
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No property type with id " + prop.Id + " found on the content type"));
                        if (ct.IsSensitiveProperty(foundOnContentType.Alias) && prop.IsSensitiveData == false)
                        {
                            //if these don't match, then we cannot continue, this user is not allowed to change this value
                            throw new HttpResponseException(HttpStatusCode.Forbidden);
                        }
                    }
                }
                else
                {
                    //if it is new, then we can just verify if any property has sensitive data turned on which is not allowed
                    if (props.Any(prop => prop.IsSensitiveData))
                    {
                        throw new HttpResponseException(HttpStatusCode.Forbidden);
                    }
                }
            }
           

            var savedCt = PerformPostSave<MemberTypeDisplay, MemberTypeSave, MemberPropertyTypeBasic>(
                contentTypeSave:            contentTypeSave,
                getContentType:             i => ct,
                saveContentType:            type => Services.MemberTypeService.Save(type));

            var display = Mapper.Map<MemberTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/memberTypeSavedHeader"),
                            string.Empty);

            return display;
        }

        
    }
}
