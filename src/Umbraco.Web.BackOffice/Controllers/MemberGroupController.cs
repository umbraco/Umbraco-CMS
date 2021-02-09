using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// An API controller used for dealing with member groups
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
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
        public ActionResult<MemberGroupDisplay> GetById(int id)
        {
            var memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                return NotFound();
            }

            var dto = _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
            return dto;
        }


        /// <summary>
        /// Gets the member group json for the member group guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberGroupDisplay> GetById(Guid id)
        {
            var memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                return NotFound();
            }

            return _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        /// <summary>
        /// Gets the member group json for the member group udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberGroupDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                return NotFound();

            var memberGroup = _memberGroupService.GetById(guidUdi.Guid);
            if (memberGroup == null)
            {
                return NotFound();
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
                return NotFound();
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

        public ActionResult<MemberGroupDisplay> PostSave(MemberGroupSave saveModel)
        {

            var id = int.Parse(saveModel.Id.ToString());
            var memberGroup = id > 0 ? _memberGroupService.GetById(id) : new MemberGroup();
            if (memberGroup == null)
            {
                return NotFound();
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
