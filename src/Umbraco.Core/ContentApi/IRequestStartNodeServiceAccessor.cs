using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestStartNodeServiceAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IRequestStartNodeService? requestStartNodeService);
}
