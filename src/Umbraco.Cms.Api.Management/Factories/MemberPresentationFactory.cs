using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberPresentationFactory : IMemberPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly IMemberGroupService _memberGroupService;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private IEnumerable<Guid>? _clientCredentialsMemberKeys;

/// <summary>
/// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.MemberPresentationFactory"/> class.
/// </summary>
/// <param name="umbracoMapper">The mapper used for mapping objects within Umbraco.</param>
/// <param name="memberService">Service for managing member entities.</param>
/// <param name="memberTypeService">Service for managing member types.</param>
/// <param name="twoFactorLoginService">Service for handling two-factor authentication for members.</param>
/// <param name="memberGroupService">Service for managing member groups.</param>
/// <param name="deliveryApiSettings">The configuration options for the Delivery API.</param>
    public MemberPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        ITwoFactorLoginService twoFactorLoginService,
        IMemberGroupService memberGroupService,
        IOptions<DeliveryApiSettings> deliveryApiSettings)
    {
        _umbracoMapper = umbracoMapper;
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _twoFactorLoginService = twoFactorLoginService;
        _memberGroupService = memberGroupService;
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="MemberResponseModel"/> for the specified <see cref="IMember"/>, including or excluding sensitive data based on the current user's permissions.
    /// </summary>
    /// <param name="member">The member entity to map to a response model.</param>
    /// <param name="currentUser">The user requesting the data, used to determine access to sensitive information.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="MemberResponseModel"/> as the result.</returns>
    public async Task<MemberResponseModel> CreateResponseModelAsync(IMember member, IUser currentUser)
    {
        MemberResponseModel responseModel = _umbracoMapper.Map<MemberResponseModel>(member)!;

        responseModel.IsTwoFactorEnabled = await _twoFactorLoginService.IsTwoFactorEnabledAsync(member.Key);
        responseModel.Kind = GetMemberKind(member.Key);
        IEnumerable<string> roles = _memberService.GetAllRoles(member.Username);

        // Get the member groups per role, so we can return the group keys
        responseModel.Groups = roles.Select(x => _memberGroupService.GetByName(x)).WhereNotNull().Select(x => x.Key).ToArray();
        return currentUser.HasAccessToSensitiveData()
            ? responseModel
            : await RemoveSensitiveDataAsync(member, responseModel);
    }

    public async Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members, IUser currentUser)
    {
        var memberResponseModels = new List<MemberResponseModel>();
        foreach (IMember member in members)
        {
            memberResponseModels.Add(await CreateResponseModelAsync(member, currentUser));
        }

        return memberResponseModels;
    }

    /// <summary>
    /// Creates a response model for a member item from the given entity.
    /// </summary>
    /// <param name="entity">The member entity to create the response model from.</param>
    /// <returns>A <see cref="MemberItemResponseModel"/> representing the member.</returns>
    public MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity)
        => CreateItemResponseModel<IMemberEntitySlim>(entity);

    /// <summary>
    /// Creates a response model for a member item based on the given member entity.
    /// </summary>
    /// <param name="entity">The member entity to create the response model from.</param>
    /// <returns>A <see cref="MemberItemResponseModel"/> representing the member.</returns>
    public MemberItemResponseModel CreateItemResponseModel(IMember entity)
        => CreateItemResponseModel<IMember>(entity);

    private MemberItemResponseModel CreateItemResponseModel<T>(T entity)
        where T : ITreeEntity
        => new MemberItemResponseModel
        {
            Id = entity.Key,
            MemberType = _umbracoMapper.Map<MemberTypeReferenceResponseModel>(entity)!,
            Variants = CreateVariantsItemResponseModels(entity),
            Kind = GetMemberKind(entity.Key)
        };

    private static IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(ITreeEntity entity)
        => new[]
        {
            new VariantItemResponseModel
            {
                Name = entity.Name ?? string.Empty,
                Culture = null
            }
        };

    private async Task<MemberResponseModel> RemoveSensitiveDataAsync(IMember member, MemberResponseModel responseModel)
    {
        // these properties are considered sensitive; some of them are not nullable, so for
        // those we can't do much more than force revert them to their default values.
        responseModel.IsApproved = false;
        responseModel.IsLockedOut = false;
        responseModel.IsTwoFactorEnabled = false;
        responseModel.FailedPasswordAttempts = 0;
        responseModel.LastLoginDate = null;
        responseModel.LastLockoutDate = null;
        responseModel.LastPasswordChangeDate = null;

        IMemberType memberType = await _memberTypeService.GetAsync(member.ContentType.Key)
                                 ?? throw new InvalidOperationException($"The member type {member.ContentType.Alias} could not be found");

        var sensitivePropertyAliases = memberType.GetSensitivePropertyTypeAliases().ToArray();

        // remove all properties whose property types are flagged as sensitive
        responseModel.Values = responseModel.Values
            .Where(valueModel => sensitivePropertyAliases.InvariantContains(valueModel.Alias) is false)
            .ToArray();

        return responseModel;
    }

    private MemberKind GetMemberKind(Guid key)
    {
        if (_clientCredentialsMemberKeys is null)
        {
            IEnumerable<string> clientCredentialsMemberUserNames = _deliveryApiSettings
                                                                       .MemberAuthorization?
                                                                       .ClientCredentialsFlow?
                                                                       .AssociatedMembers
                                                                       .Select(m => m.UserName).ToArray()
                                                                   ?? [];

            _clientCredentialsMemberKeys = clientCredentialsMemberUserNames
                .Select(_memberService.GetByUsername)
                .WhereNotNull()
                .Select(m => m.Key).ToArray();
        }

        return _clientCredentialsMemberKeys.Contains(key) ? MemberKind.Api : MemberKind.Default;
    }
}
