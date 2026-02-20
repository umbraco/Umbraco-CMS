namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="ICurrentMemberClaimsProvider"/> that returns empty claims.
/// </summary>
public class NoopCurrentMemberClaimsProvider : ICurrentMemberClaimsProvider
{
    /// <inheritdoc />
    public Task<Dictionary<string, object>> GetClaimsAsync() => Task.FromResult(new Dictionary<string, object>());
}
