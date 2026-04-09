using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Serves as a base class for implementing client credentials management functionality within the security framework.
/// </summary>
public abstract class ClientCredentialsManagerBase
{
    protected abstract string ClientIdPrefix { get; }

    protected string SafeClientId(string clientId) => clientId.EnsureStartsWith($"{ClientIdPrefix}-");
}
