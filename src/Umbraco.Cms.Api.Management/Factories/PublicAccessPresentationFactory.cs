using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Default implementation of <see cref="IPublicAccessPresentationFactory"/> that converts
/// <see cref="PublicAccessEntry"/> domain models to presentation response models and vice versa.
/// </summary>
public class PublicAccessPresentationFactory : IPublicAccessPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IMemberService _memberService;
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberRoleManager _memberRoleManager;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">The entity service for resolving entity keys.</param>
    /// <param name="memberService">The member service for looking up members by username.</param>
    /// <param name="mapper">The Umbraco mapper for mapping entities to response models.</param>
    /// <param name="memberRoleManager">The member role manager for resolving member groups.</param>
    /// <param name="memberPresentationFactory">The member presentation factory for creating member item response models.</param>
    public PublicAccessPresentationFactory(
        IEntityService entityService,
        IMemberService memberService,
        IUmbracoMapper mapper,
        IMemberRoleManager memberRoleManager,
        IMemberPresentationFactory memberPresentationFactory)
    {
        _entityService = entityService;
        _memberService = memberService;
        _mapper = mapper;
        _memberRoleManager = memberRoleManager;
        _memberPresentationFactory = memberPresentationFactory;
    }

    /// <inheritdoc/>
    public Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry, Guid contentKey)
    {
        Attempt<Guid> protectedNodeKeyAttempt = _entityService.GetKey(entry.ProtectedNodeId, UmbracoObjectTypes.Document);

        if (protectedNodeKeyAttempt.Success is false)
        {
            return Attempt.FailWithStatus<PublicAccessResponseModel?, PublicAccessOperationStatus>(PublicAccessOperationStatus.ContentNotFound, null);
        }

        // While the obsolete overload is still supported, let's use it.
        // TODO (V18): Remove the obsolete overload and move its logic here.
#pragma warning disable CS0618 // Type or member is obsolete
        Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> baseResponseAttempt = CreatePublicAccessResponseModel(entry);
#pragma warning restore CS0618 // Type or member is obsolete

        if (baseResponseAttempt.Success is false)
        {
            return baseResponseAttempt;
        }

        if (protectedNodeKeyAttempt.Result.Equals(contentKey) is false && baseResponseAttempt.Result is not null)
        {
            baseResponseAttempt.Result.IsProtectedByAncestor = true;
        }

        return Attempt.SucceedWithStatus<PublicAccessResponseModel?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, baseResponseAttempt.Result);
    }

    /// <inheritdoc/>
    [Obsolete("Plase use the overload taking all parameters. Scheduled for removal in Umbraco 19.")]
    public Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry)
    {
        Attempt<Guid> loginNodeKeyAttempt = _entityService.GetKey(entry.LoginNodeId, UmbracoObjectTypes.Document);
        Attempt<Guid> noAccessNodeKeyAttempt = _entityService.GetKey(entry.NoAccessNodeId, UmbracoObjectTypes.Document);

        if (loginNodeKeyAttempt.Success is false)
        {
            return Attempt.FailWithStatus<PublicAccessResponseModel?, PublicAccessOperationStatus>(PublicAccessOperationStatus.LoginNodeNotFound, null);
        }

        if (noAccessNodeKeyAttempt.Success is false)
        {
            return Attempt.FailWithStatus<PublicAccessResponseModel?, PublicAccessOperationStatus>(PublicAccessOperationStatus.ErrorNodeNotFound, null);
        }

        // unwrap the current public access setup for the client
        // - this API method is the single point of entry for both "modes" of public access (single user and role based)
        var usernames = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
            .Select(rule => rule.RuleValue)
            .ToArray();

        MemberItemResponseModel[] members = usernames
            .Select(username => _memberService.GetByUsername(username))
            .WhereNotNull()
            .Select(_memberPresentationFactory.CreateItemResponseModel)
            .ToArray();

        var allGroups = _memberRoleManager.Roles.Where(x => x.Name != null).ToDictionary(x => x.Name!);
        IEnumerable<UmbracoIdentityRole> identityRoles = entry.Rules
            .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
            .Select(rule =>
                rule.RuleValue is not null && allGroups.TryGetValue(rule.RuleValue, out UmbracoIdentityRole? memberRole)
                    ? memberRole
                    : null)
            .WhereNotNull()
            .ToArray();

        IEnumerable<IEntitySlim> groupsEntities = identityRoles.Any()
            ? _entityService.GetAll(UmbracoObjectTypes.MemberGroup, identityRoles.Select(x => Convert.ToInt32(x.Id)).ToArray())
            : Enumerable.Empty<IEntitySlim>();
        MemberGroupItemResponseModel[] memberGroups = groupsEntities.Select(x => _mapper.Map<MemberGroupItemResponseModel>(x)!).ToArray();

        var responseModel = new PublicAccessResponseModel
        {
            Members = members,
            Groups = memberGroups,
            LoginDocument = new ReferenceByIdModel(loginNodeKeyAttempt.Result),
            ErrorDocument = new ReferenceByIdModel(noAccessNodeKeyAttempt.Result),
        };

        return Attempt.SucceedWithStatus<PublicAccessResponseModel?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, responseModel);
    }

    /// <inheritdoc/>
    public PublicAccessEntrySlim CreatePublicAccessEntrySlim(PublicAccessRequestModel requestModel, Guid contentKey) =>
        new()
        {
            ContentId = contentKey,
            MemberGroupNames = requestModel.MemberGroupNames,
            MemberUserNames = requestModel.MemberUserNames,
            ErrorPageId = requestModel.ErrorDocument.Id,
            LoginPageId = requestModel.LoginDocument.Id,
        };
}
