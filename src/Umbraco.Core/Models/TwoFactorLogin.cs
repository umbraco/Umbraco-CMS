using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public class TwoFactorLogin : EntityBase, ITwoFactorLogin
{
    public bool Confirmed { get; set; }

    public string ProviderName { get; set; } = null!;

    public string Secret { get; set; } = null!;

    public Guid UserOrMemberKey { get; set; }
}
