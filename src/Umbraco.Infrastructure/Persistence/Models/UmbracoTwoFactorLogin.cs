namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoTwoFactorLogin
{
    public int Id { get; set; }

    public Guid UserOrMemberKey { get; set; }

    public string ProviderName { get; set; } = null!;

    public string Secret { get; set; } = null!;
}
