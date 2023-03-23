using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestStartItemServiceAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IRequestStartItemService? requestStartNodeService);
}
