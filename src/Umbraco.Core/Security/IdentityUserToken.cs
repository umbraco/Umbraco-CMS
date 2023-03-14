using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Security;

public class IdentityUserToken : EntityBase, IIdentityUserToken
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentityUserToken" /> class.
    /// </summary>
    public IdentityUserToken(string loginProvider, string? name, string? value, string? userId)
    {
        LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UserId = userId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentityUserToken" /> class.
    /// </summary>
    public IdentityUserToken(int id, string? loginProvider, string? name, string? value, string userId, DateTime createDate)
    {
        Id = id;
        LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UserId = userId;
        CreateDate = createDate;
    }

    /// <inheritdoc />
    public string LoginProvider { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string Value { get; set; }

    /// <inheritdoc />
    public string? UserId { get; set; }
}
