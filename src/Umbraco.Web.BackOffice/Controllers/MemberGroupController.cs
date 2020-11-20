using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Models.ContentEditing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// An API controller used for dealing with member groups
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
    public class MemberGroupController : UmbracoAuthorizedJsonController
    {
        private readonly IMemberGroupService _memberGroupService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly ILocalizedTextService _localizedTextService;

        public MemberGroupController(
            IMemberGroupService memberGroupService,
            UmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService
            )
        {
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _localizedTextService =
                localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        /// <summary>
        /// Gets the member group json for the member group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public MemberGroupDisplay GetById(int id)
        {
            var memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
            return dto;
        }


        /// <summary>
        /// Gets the member group json for the member group guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public MemberGroupDisplay GetById(Guid id)
        {
            var memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        /// <summary>
        /// Gets the member group json for the member group udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public MemberGroupDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var memberGroup = _memberGroupService.GetById(guidUdi.Guid);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        public IEnumerable<MemberGroupDisplay> GetByIds([FromQuery]int[] ids)
        {
            return _memberGroupService.GetByIds(ids)
                    .Select(_umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>);
        }

        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            _memberGroupService.Delete(memberGroup);
            return Ok();
        }

        public IEnumerable<MemberGroupDisplay> GetAllGroups()
        {
            return _memberGroupService.GetAll()
                    .Select(_umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>);
        }

        public MemberGroupDisplay GetEmpty()
        {
            var item = new MemberGroup();
            return _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(item);
        }

        public MemberGroupDisplay PostSave(MemberGroupSave saveModel)
        {

            var id = int.Parse(saveModel.Id.ToString());
            var memberGroup = id > 0 ? _memberGroupService.GetById(id) : new MemberGroup();
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            memberGroup.Name = saveModel.Name;
            _memberGroupService.Save(memberGroup);

            var display = _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);

            display.AddSuccessNotification(
                            _localizedTextService.Localize("speechBubbles/memberGroupSavedHeader"),
                            string.Empty);

            return display;
        }
    }
}
