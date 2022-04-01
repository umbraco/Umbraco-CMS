using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// An API controller used for dealing with member types
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class MemberTypeController : ContentTypeControllerBase<IMemberType>
    {
        private readonly IMemberTypeService _memberTypeService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly ILocalizedTextService _localizedTextService;

        public MemberTypeController(
            ICultureDictionary cultureDictionary,
            EditorValidatorCollection editorValidatorCollection,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMemberTypeService memberTypeService,
            IUmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
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
        public ActionResult<MemberTypeDisplay?> GetById(int id)
        {
            var mt = _memberTypeService.Get(id);
            if (mt == null)
            {
                return NotFound();
            }

            var dto =_umbracoMapper.Map<IMemberType, MemberTypeDisplay>(mt);
            return dto;
        }

        /// <summary>
        /// Gets the member type a given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberTypeDisplay?> GetById(Guid id)
        {
            var memberType = _memberTypeService.Get(id);
            if (memberType == null)
            {
                return NotFound();
            }

            var dto = _umbracoMapper.Map<IMemberType, MemberTypeDisplay>(memberType);
            return dto;
        }

        /// <summary>
        /// Gets the member type a given udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberTypeDisplay?> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                return NotFound();

            var memberType = _memberTypeService.Get(guidUdi.Guid);
            if (memberType == null)
            {
                return NotFound();
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
        public IActionResult DeleteById(int id)
        {
            var foundType = _memberTypeService.Get(id);
            if (foundType == null)
            {
                return NotFound();
            }

            _memberTypeService.Delete(foundType, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);
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
        public IActionResult GetAvailableCompositeMemberTypes(int contentTypeId,
            [FromQuery]string[] filterContentTypes,
            [FromQuery]string[] filterPropertyTypes)
        {
            var actionResult = PerformGetAvailableCompositeContentTypes(contentTypeId,
                UmbracoObjectTypes.MemberType, filterContentTypes, filterPropertyTypes,
                false);

            if (!(actionResult.Result is null))
            {
                return actionResult.Result;
            }

            var result = actionResult.Value?
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Ok(result);
        }

        public MemberTypeDisplay? GetEmpty()
        {
            var ct = new MemberType(_shortStringHelper, -1);
            ct.Icon = Constants.Icons.Member;

            var dto =_umbracoMapper.Map<IMemberType, MemberTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all member types
        /// </summary>
        [Obsolete("Use MemberTypeQueryController.GetAllTypes instead as it only requires AuthorizationPolicies.TreeAccessMembersOrMemberTypes and not both this and AuthorizationPolicies.TreeAccessMemberTypes")]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
        public IEnumerable<ContentTypeBasic> GetAllTypes()
        {
            return _memberTypeService.GetAll()
                               .Select(_umbracoMapper.Map<IMemberType, ContentTypeBasic>).WhereNotNull();
        }

        public ActionResult<MemberTypeDisplay?> PostSave(MemberTypeSave contentTypeSave)
        {
            //get the persisted member type
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            var ct = ctId > 0 ? _memberTypeService.Get(ctId) : null;

            if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.HasAccessToSensitiveData() == false)
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
                            return Forbid();
                        }
                    }
                }
                else
                {
                    //if it is new, then we can just verify if any property has sensitive data turned on which is not allowed
                    if (props.Any(prop => prop.IsSensitiveData))
                    {
                        return Forbid();
                    }
                }
            }


            var savedCt = PerformPostSave<MemberTypeDisplay, MemberTypeSave, MemberPropertyTypeBasic>(
                contentTypeSave:            contentTypeSave,
                getContentType:             i => ct,
                saveContentType:            type => _memberTypeService.Save(type));

            if (!(savedCt.Result is null))
            {
                return savedCt.Result;
            }

            var display =_umbracoMapper.Map<MemberTypeDisplay>(savedCt.Value);

            display?.AddSuccessNotification(
                _localizedTextService.Localize("speechBubbles","memberTypeSavedHeader"),
                            string.Empty);

            return display;
        }
    }
}
