using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi.Accessors;

public class NoopRequestStartNodeServiceAccessor : IRequestStartNodeServiceAccessor
{
    public bool TryGetValue([NotNullWhen(true)] out IRequestStartNodeService? requestStartNodeService)
    {
        requestStartNodeService = new NoopRequestStartNodeService();
        return true;
    }
}
