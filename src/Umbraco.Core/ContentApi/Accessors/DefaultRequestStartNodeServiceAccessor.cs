using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi.Accessors;

public class DefaultRequestStartNodeServiceAccessor : IRequestStartNodeServiceAccessor
{
    public bool TryGetValue([NotNullWhen(true)] out IRequestStartNodeService? requestStartNodeService)
    {
        requestStartNodeService = new DefaultRequestStartNodeService();
        return true;
    }
}
