using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class PublicAccessController : BackOfficeNotificationsController
{
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IMemberRoleManager _memberRoleManager;
    private readonly IMemberService _memberService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IUmbracoMapper _umbracoMapper;

    public PublicAccessController(
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IEntityService entityService,
        IMemberService memberService,
        IUmbracoMapper umbracoMapper,
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
        IContent? content = _contentService.GetById(contentId);
        if (content == null)
        {
            return NotFound();
        }

        PublicAccessEntry? entry = _publicAccessService.GetEntryForContent(content);
        if (entry == null || entry.ProtectedNodeId != content.Id)
        {
            return Ok();
        }

        var nodes = _entityService
            .GetAll(UmbracoObjectTypes.Document, entry.LoginNodeId, entry.NoAccessNodeId)
            .ToDictionary(x => x.Id);

        if (!nodes.TryGetValue(entry.LoginNodeId, out IEntitySlim? loginPageEntity))
        {
            throw new InvalidOperationException($"Login node with id ${entry.LoginNodeId} was not found");
        }

        if (!nodes.TryGetValue(entry.NoAccessNodeId, out IEntitySlim? errorPageEntity))
        {
            throw new InvalidOperationException($"Error node with id ${entry.LoginNodeId} was not found");
        }

        // unwrap the current public access setup for the client
        // - this API method is the single point of entry for both "modes" of public access (single user and role based)
        var usernames = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
            .Select(rule => rule.RuleValue)
            .ToArray();

        MemberDisplay[] members = usernames
            .Select(username => _memberService.GetByUsername(username))
            .Select(_umbracoMapper.Map<MemberDisplay>)
            .WhereNotNull()
            .ToArray();

        var allGroups = _memberRoleManager.Roles.Where(x => x.Name != null).ToDictionary(x => x.Name!);
        MemberGroupDisplay[] groups = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
            .Select(rule =>
                rule.RuleValue is not null && allGroups.TryGetValue(rule.RuleValue, out UmbracoIdentityRole? memberRole)
                    ? memberRole
                    : null)
            .Select(_umbracoMapper.Map<MemberGroupDisplay>)
            .WhereNotNull()
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
    public IActionResult PostPublicAccess(int contentId, [FromQuery(Name = "groups[]")] string[] groups,
        [FromQuery(Name = "usernames[]")] string[] usernames, int loginPageId, int errorPageId)
    {
        if ((groups == null || groups.Any() == false) && (usernames == null || usernames.Any() == false))
        {
            return BadRequest();
        }

        IContent? content = _contentService.GetById(contentId);
        IContent? loginPage = _contentService.GetById(loginPageId);
        IContent? errorPage = _contentService.GetById(errorPageId);
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

        PublicAccessEntry? entry = _publicAccessService.GetEntryForContent(content);

        if (entry == null || entry.ProtectedNodeId != content.Id)
        {
            entry = new PublicAccessEntry(content, loginPage, errorPage, new List<PublicAccessRule>());

            if (candidateRuleValues is not null)
            {
                foreach (var ruleValue in candidateRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
            }
        }
        else
        {
            entry.LoginNodeId = loginPage.Id;
            entry.NoAccessNodeId = errorPage.Id;

            PublicAccessRule[] currentRules = entry.Rules.ToArray();
            IEnumerable<PublicAccessRule> obsoleteRules = currentRules.Where(rule =>
                rule.RuleType != newRuleType
                || candidateRuleValues?.Contains(rule.RuleValue) == false
            );
            IEnumerable<string>? newRuleValues = candidateRuleValues?.Where(group =>
                currentRules.Any(rule =>
                    rule.RuleType == newRuleType
                    && rule.RuleValue == group
                ) == false
            );
            foreach (PublicAccessRule rule in obsoleteRules)
            {
                entry.RemoveRule(rule);
            }

            if (newRuleValues is not null)
            {
                foreach (var ruleValue in newRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
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
        IContent? content = _contentService.GetById(contentId);
        if (content == null)
        {
            return NotFound();
        }

        PublicAccessEntry? entry = _publicAccessService.GetEntryForContent(content);
        if (entry == null)
        {
            return Ok();
        }

        return _publicAccessService.Delete(entry).Success
            ? Ok()
            : Problem();
    }
}
