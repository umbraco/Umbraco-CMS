﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models.ContentEditing;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Mapping;
using Umbraco.Core.Security;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with member types
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [UmbracoTreeAuthorize(new string[] { Constants.Trees.MemberTypes, Constants.Trees.Members})]
    public class MemberTypeController : ContentTypeControllerBase<IMemberType>
    {
        private readonly IMemberTypeService _memberTypeService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly ILocalizedTextService _localizedTextService;

        public MemberTypeController(
            ICultureDictionary cultureDictionary,
            EditorValidatorCollection editorValidatorCollection,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMemberTypeService memberTypeService,
            UmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            IShortStringHelper shortStringHelper)
            : base(cultureDictionary,
                editorValidatorCollection,
                contentTypeService,
                mediaTypeService,
                memberTypeService,
                umbracoMapper,
                localizedTextService)
        {
            _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _localizedTextService =
                localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        /// <summary>
        /// Gets the member type a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        [DetermineAmbiguousActionByPassingParameters]
        public MemberTypeDisplay GetById(int id)
        {
            var ct = _memberTypeService.Get(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto =_umbracoMapper.Map<IMemberType, MemberTypeDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Gets the member type a given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        [DetermineAmbiguousActionByPassingParameters]
        public MemberTypeDisplay GetById(Guid id)
        {
            var memberType = _memberTypeService.Get(id);
            if (memberType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMemberType, MemberTypeDisplay>(memberType);
            return dto;
        }

        /// <summary>
        /// Gets the member type a given udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        [DetermineAmbiguousActionByPassingParameters]
        public MemberTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var memberType = _memberTypeService.Get(guidUdi.Guid);
            if (memberType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMemberType, MemberTypeDisplay>(memberType);
            return dto;
        }

        /// <summary>
        /// Deletes a document type with a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        public IActionResult DeleteById(int id)
        {
            var foundType = _memberTypeService.Get(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            _memberTypeService.Delete(foundType, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.Id);
            return Ok();
        }

        /// <summary>
        /// Returns the available compositions for this content type
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
        public IActionResult GetAvailableCompositeMemberTypes(int contentTypeId,
            [FromQuery]string[] filterContentTypes,
            [FromQuery]string[] filterPropertyTypes)
        {
            var result = PerformGetAvailableCompositeContentTypes(contentTypeId, UmbracoObjectTypes.MemberType, filterContentTypes, filterPropertyTypes, false)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Ok(result);
        }

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        public MemberTypeDisplay GetEmpty()
        {
            var ct = new MemberType(_shortStringHelper, -1);
            ct.Icon = Constants.Icons.Member;

            var dto =_umbracoMapper.Map<IMemberType, MemberTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all member types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAllTypes()
        {
            return _memberTypeService.GetAll()
                               .Select(_umbracoMapper.Map<IMemberType, ContentTypeBasic>);
        }

        [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
        public ActionResult<MemberTypeDisplay> PostSave(MemberTypeSave contentTypeSave)
        {
            //get the persisted member type
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            var ct = ctId > 0 ? _memberTypeService.Get(ctId) : null;

            if (_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.HasAccessToSensitiveData() == false)
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
                        {
                            return NotFound(new
                                { Message = "No property type with id " + prop.Id + " found on the content type" });
                        }

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
                saveContentType:            type => _memberTypeService.Save(type));

            var display =_umbracoMapper.Map<MemberTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                _localizedTextService.Localize("speechBubbles/memberTypeSavedHeader"),
                            string.Empty);

            return display;
        }


    }
}
