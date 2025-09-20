namespace Umbraco.Cms.Core.DeliveryApi;

public class NoopCurrentMemberClaimsProvider : ICurrentMemberClaimsProvider
{
    public Task<Dictionary<string, object>> GetClaimsAsync() => Task.FromResult(new Dictionary<string, object>());
}
