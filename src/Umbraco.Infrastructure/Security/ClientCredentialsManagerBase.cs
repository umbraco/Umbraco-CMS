using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

public abstract class ClientCredentialsManagerBase
{
    protected abstract string ClientIdPrefix { get; }

    protected string SafeClientId(string clientId) => clientId.EnsureStartsWith($"{ClientIdPrefix}-");
}
