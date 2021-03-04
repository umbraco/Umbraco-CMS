using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
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
        private readonly RoleManager<IdentityRole<string>> _roleManager;
     
        public MemberGroupController(
            IMemberGroupService memberGroupService,
            UmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            RoleManager<IdentityRole<string>> roleManager
            )
        {
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        /// <summary>
        /// Gets the member group json for the member group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberGroupDisplay> GetById(int id)
        {
            IdentityRole<string> memberGroup = _roleManager.FindByIdAsync(id.ToString()).Result;
            if (memberGroup == null)
            {
                return NotFound();
            }

            MemberGroupDisplay dto = _umbracoMapper.Map<IdentityRole<string>, MemberGroupDisplay>(memberGroup);
            return dto;
        }


        /// <summary>
        /// Gets the member group json for the member group guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<MemberGroupDisplay> GetById(Guid id)
        {
            IMemberGroup memberGroup = _memberGroupService.GetById(id);
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
            {
                return NotFound();
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(guidUdi.Guid);
            if (memberGroup == null)
            {
                return NotFound();
            }

            return _umbracoMapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        public IEnumerable<MemberGroupDisplay> GetByIds([FromQuery]int[] ids)
        {
            var roles = new List<IdentityRole<string>>();

            foreach (int id in ids)
            {
                Task<IdentityRole<string>> role = _roleManager.FindByIdAsync(id.ToString());
                roles.Add(role.Result);
            }

            return roles.Select(x=> _umbracoMapper.Map<IdentityRole<string>, MemberGroupDisplay>(x));
        }

        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            IMemberGroup memberGroup = _memberGroupService.GetById(id);
            if (memberGroup == null)
            {
                return NotFound();
            }

            _memberGroupService.Delete(memberGroup);
            return Ok();
        }

        public IEnumerable<MemberGroupDisplay> GetAllGroups() => _roleManager.Roles.Select(x => _umbracoMapper.Map<IdentityRole<string>, MemberGroupDisplay>(x));

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
