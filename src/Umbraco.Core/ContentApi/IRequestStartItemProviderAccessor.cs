using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestStartItemProviderAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IRequestStartItemProvider? requestStartItemProvider);
}
