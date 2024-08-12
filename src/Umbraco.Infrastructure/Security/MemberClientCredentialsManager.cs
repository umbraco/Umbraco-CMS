using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Security;

public sealed class MemberClientCredentialsManager : ClientCredentialsManagerBase, IMemberClientCredentialsManager
{
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly IMemberManager _memberManager;
    private readonly ILogger<MemberClientCredentialsManager> _logger;

    public MemberClientCredentialsManager(
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        IMemberManager memberManager,
        ILogger<MemberClientCredentialsManager> logger)
    {
        _deliveryApiSettings = deliveryApiSettings.Value;
        _memberManager = memberManager;
        _logger = logger;
    }

    protected override string ClientIdPrefix => Constants.OAuthClientIds.Member;

    public Task<IEnumerable<MemberClientCredentials>> GetAllAsync()
    {
        IEnumerable<MemberClientCredentials> result = IsDisabled()
            ? Enumerable.Empty<MemberClientCredentials>()
            : _deliveryApiSettings
                .MemberAuthorization!
                .ClientCredentialsFlow!
                .AssociatedMembers
                .Select(m => new MemberClientCredentials
                {
                    ClientId = SafeClientId(m.ClientId),
                    ClientSecret = m.ClientSecret,
                    UserName = m.UserName
                })
                .ToArray();

        return Task.FromResult(result);
    }

    public async Task<MemberIdentityUser?> FindMemberAsync(string clientId)
    {
        clientId = SafeClientId(clientId);
        var userName = IsDisabled()
            ? null
            : _deliveryApiSettings
                .MemberAuthorization?
                .ClientCredentialsFlow?
                .AssociatedMembers
                .FirstOrDefault(m => SafeClientId(m.ClientId) == clientId)?
                .UserName;

        if (userName is null)
        {
            return null;
        }

        MemberIdentityUser? user = await _memberManager.FindByNameAsync(userName);
        if (user is null)
        {
            _logger.LogWarning("The member with username {userName} could not be retrieved by the member manager", userName);
        }

        return user;
    }

    private bool IsDisabled() => _deliveryApiSettings.MemberAuthorization?.ClientCredentialsFlow?.Enabled is not true
                                 || _deliveryApiSettings.MemberAuthorization.ClientCredentialsFlow.AssociatedMembers.Any() is false;
}
