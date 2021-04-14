using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
    public class PublicAccessController : BackOfficeNotificationsController
    {
        private readonly IContentService _contentService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IEntityService _entityService;
        private readonly IMemberService _memberService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IMemberRoleManager _memberRoleManager;

        public PublicAccessController(
            IPublicAccessService publicAccessService,
            IContentService contentService,
            IEntityService entityService,
            IMemberService memberService,
            UmbracoMapper umbracoMapper,
            IMemberRoleManager memberRoleManager)
        {
            _contentService = contentService;
            _publicAccessService = publicAccessService;
            _entityService = entityService;
            _memberService = memberService;
            _umbracoMapper = umbracoMapper;
            _memberRoleManager = memberRoleManager;
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpGet]
        public ActionResult<PublicAccess> GetPublicAccess(int contentId)
        {
            IContent content = _contentService.GetById(contentId);
            if (content == null)
            {
                return NotFound();
            }

            PublicAccessEntry entry = _publicAccessService.GetEntryForContent(content);
            if (entry == null || entry.ProtectedNodeId != content.Id)
            {
                return Ok();
            }

            IEntitySlim loginPageEntity = _entityService.Get(entry.LoginNodeId, UmbracoObjectTypes.Document);
            IEntitySlim errorPageEntity = _entityService.Get(entry.NoAccessNodeId, UmbracoObjectTypes.Document);

            // unwrap the current public access setup for the client
            // - this API method is the single point of entry for both "modes" of public access (single user and role based)
            var usernames = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
                .Select(rule => rule.RuleValue)
                .ToArray();

            MemberDisplay[] members = usernames
                .Select(username => _memberService.GetByUsername(username))
                .Where(member => member != null)
                .Select(_umbracoMapper.Map<MemberDisplay>)
                .ToArray();

            var allGroups = _memberRoleManager.Roles.ToDictionary(x => x.Name);
            MemberGroupDisplay[] groups = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
                .Select(rule => allGroups.TryGetValue(rule.RuleValue, out UmbracoIdentityRole memberRole) ? memberRole : null)
                .Where(x => x != null)
                .Select(_umbracoMapper.Map<MemberGroupDisplay>)
                .ToArray();

            return new PublicAccess
            {
                Members = members,
                Groups = groups,
                LoginPage = loginPageEntity != null ? _umbracoMapper.Map<EntityBasic>(loginPageEntity) : null,
                ErrorPage = errorPageEntity != null ? _umbracoMapper.Map<EntityBasic>(errorPageEntity) : null
            };
        }

        // set up public access using role based access
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpPost]
        public IActionResult PostPublicAccess(int contentId, [FromQuery(Name = "groups[]")] string[] groups, [FromQuery(Name = "usernames[]")] string[] usernames, int loginPageId, int errorPageId)
        {
            if ((groups == null || groups.Any() == false) && (usernames == null || usernames.Any() == false))
            {
                return BadRequest();
            }

            var content = _contentService.GetById(contentId);
            var loginPage = _contentService.GetById(loginPageId);
            var errorPage = _contentService.GetById(errorPageId);
            if (content == null || loginPage == null || errorPage == null)
            {
                return BadRequest();
            }

            var isGroupBased = groups != null && groups.Any();
            var candidateRuleValues = isGroupBased
                ? groups
                : usernames;
            var newRuleType = isGroupBased
                ? Constants.Conventions.PublicAccess.MemberRoleRuleType
                : Constants.Conventions.PublicAccess.MemberUsernameRuleType;

            var entry = _publicAccessService.GetEntryForContent(content);

            if (entry == null || entry.ProtectedNodeId != content.Id)
            {
                entry = new PublicAccessEntry(content, loginPage, errorPage, new List<PublicAccessRule>());

                foreach (var ruleValue in candidateRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
            }
            else
            {
                entry.LoginNodeId = loginPage.Id;
                entry.NoAccessNodeId = errorPage.Id;

                var currentRules = entry.Rules.ToArray();
                var obsoleteRules = currentRules.Where(rule =>
                    rule.RuleType != newRuleType
                    || candidateRuleValues.Contains(rule.RuleValue) == false
                );
                var newRuleValues = candidateRuleValues.Where(group =>
                    currentRules.Any(rule =>
                        rule.RuleType == newRuleType
                        && rule.RuleValue == group
                    ) == false
                );
                foreach (var rule in obsoleteRules)
                {
                    entry.RemoveRule(rule);
                }
                foreach (var ruleValue in newRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
            }

            return _publicAccessService.Save(entry).Success
                ? Ok()
                : Problem();
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpPost]
        public IActionResult RemovePublicAccess(int contentId)
        {
            var content = _contentService.GetById(contentId);
            if (content == null)
            {
                return NotFound();
            }

            var entry = _publicAccessService.GetEntryForContent(content);
            if (entry == null)
            {
                return Ok();
            }

            return _publicAccessService.Delete(entry).Success
                ? Ok()
                : Problem();
        }
    }
}
