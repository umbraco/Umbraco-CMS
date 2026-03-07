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
/// Provides methods to create models for public access presentation.
/// </summary>
public class PublicAccessPresentationFactory : IPublicAccessPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IMemberService _memberService;
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberRoleManager _memberRoleManager;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.PublicAccessPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">Service used for accessing and managing entities within Umbraco.</param>
    /// <param name="memberService">Service for managing member-related operations.</param>
    /// <param name="mapper">The Umbraco object mapper for mapping between models.</param>
    /// <param name="memberRoleManager">Manager responsible for handling member roles.</param>
    /// <param name="memberPresentationFactory">Factory for creating member presentation models.</param>
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

    /// <summary>
    /// Creates a <see cref="PublicAccessResponseModel"/> from the specified <see cref="PublicAccessEntry"/>.
    /// </summary>
    /// <param name="entry">The public access entry containing the rules and node references to build the response model.</param>
    /// <returns>
    /// An <see cref="Attempt{PublicAccessResponseModel?, PublicAccessOperationStatus}"/> indicating the outcome of the operation.
    /// If successful, contains the populated response model; otherwise, contains the failure status and <c>null</c> as the model.
    /// </returns>
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

    /// <summary>
    /// Creates a lightweight <see cref="PublicAccessEntrySlim"/> instance based on the specified public access request model and content key.
    /// </summary>
    /// <param name="requestModel">The model containing member group names, member user names, and document references for public access configuration.</param>
    /// <param name="contentKey">The unique identifier of the content to be protected.</param>
    /// <returns>A <see cref="PublicAccessEntrySlim"/> representing the configured public access entry.</returns>
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
