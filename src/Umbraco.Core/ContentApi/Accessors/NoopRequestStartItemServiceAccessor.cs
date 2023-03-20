using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi.Accessors;

public class NoopRequestStartItemServiceAccessor : IRequestStartItemServiceAccessor
{
    public bool TryGetValue([NotNullWhen(true)] out IRequestStartItemService? requestStartNodeService)
    {
        requestStartNodeService = new NoopRequestStartItemService();
        return true;
    }
}
