using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Entity type for a user's login (i.e. Facebook, Google)
/// </summary>
public class IdentityUserLogin : EntityBase, IIdentityUserLogin
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentityUserLogin" /> class.
    /// </summary>
    public IdentityUserLogin(string loginProvider, string providerKey, string userId)
    {
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        UserId = userId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentityUserLogin" /> class.
    /// </summary>
    public IdentityUserLogin(int id, string loginProvider, string providerKey, string userId, DateTime createDate)
    {
        Id = id;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        UserId = userId;
        CreateDate = createDate;
    }

    /// <inheritdoc />
    public string LoginProvider { get; set; }

    /// <inheritdoc />
    public string ProviderKey { get; set; }

    /// <inheritdoc />
    public string UserId { get; set; }

    /// <inheritdoc />
    public string? UserData { get; set; }
}
