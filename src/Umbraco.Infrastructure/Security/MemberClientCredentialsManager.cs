using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides functionality for managing client credentials for members within the Umbraco CMS security framework.
/// </summary>
public sealed class MemberClientCredentialsManager : ClientCredentialsManagerBase, IMemberClientCredentialsManager
{
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly IMemberManager _memberManager;
    private readonly ILogger<MemberClientCredentialsManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberClientCredentialsManager"/> class.
    /// </summary>
    /// <param name="deliveryApiSettings">The <see cref="IOptions{DeliveryApiSettings}"/> containing the delivery API settings.</param>
    /// <param name="memberManager">The <see cref="IMemberManager"/> instance used to manage members.</param>
    /// <param name="logger">The <see cref="ILogger{MemberClientCredentialsManager}"/> instance for logging.</param>
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

    /// <summary>
    /// Asynchronously retrieves all member client credentials.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="MemberClientCredentials"/>. If the manager is disabled, an empty collection is returned.
    /// </returns>
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

    /// <summary>
    /// Asynchronously finds a member associated with the specified client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier used to locate the associated member.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="MemberIdentityUser"/> if a matching member is found; otherwise, <c>null</c>.
    /// </returns>
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
