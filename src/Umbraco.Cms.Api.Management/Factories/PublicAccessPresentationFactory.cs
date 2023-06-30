using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class PublicAccessPresentationFactory : IPublicAccessPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IMemberService _memberService;
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberRoleManager _memberRoleManager;

    public PublicAccessPresentationFactory(IEntityService entityService, IMemberService memberService, IUmbracoMapper mapper, IMemberRoleManager memberRoleManager)
    {
        _entityService = entityService;
        _memberService = memberService;
        _mapper = mapper;
        _memberRoleManager = memberRoleManager;
    }

    public PublicAccessResponseModel CreatePublicAccessResponseModel(PublicAccessEntry entry)
    {
        var nodes = _entityService
            .GetAll(UmbracoObjectTypes.Document, entry.LoginNodeId, entry.NoAccessNodeId)
            .ToDictionary(x => x.Id);

        if (!nodes.TryGetValue(entry.LoginNodeId, out IEntitySlim? loginPageEntity))
        {
            throw new InvalidOperationException($"Login node with id ${entry.LoginNodeId} was not found");
        }

        if (!nodes.TryGetValue(entry.NoAccessNodeId, out IEntitySlim? errorPageEntity))
        {
            throw new InvalidOperationException($"Error node with id ${entry.NoAccessNodeId} was not found");
        }

        // unwrap the current public access setup for the client
        // - this API method is the single point of entry for both "modes" of public access (single user and role based)
        var usernames = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
            .Select(rule => rule.RuleValue)
            .ToArray();

        MemberItemResponseModel[] members = usernames
            .Select(username => _memberService.GetByUsername(username))
            .Select(_mapper.Map<MemberItemResponseModel>)
            .WhereNotNull()
            .ToArray();

        var allGroups = _memberRoleManager.Roles.Where(x => x.Name != null).ToDictionary(x => x.Name!);
        MemberGroupItemResponseModel[] groups = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
            .Select(rule =>
                rule.RuleValue is not null && allGroups.TryGetValue(rule.RuleValue, out UmbracoIdentityRole? memberRole)
                    ? memberRole
                    : null)
            .Select(_mapper.Map<MemberGroupItemResponseModel>)
            .WhereNotNull()
            .ToArray();

        return new PublicAccessResponseModel
        {
            Members = members,
            Groups = groups,
            LoginPage = loginPageEntity is not null ? _mapper.Map<ContentTreeItemResponseModel>(loginPageEntity) : null,
            ErrorPage = errorPageEntity != null ? _mapper.Map<ContentTreeItemResponseModel>(errorPageEntity) : null
        };
    }
}
