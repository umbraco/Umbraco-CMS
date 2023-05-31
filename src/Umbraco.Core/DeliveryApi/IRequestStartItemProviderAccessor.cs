using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestStartItemProviderAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IRequestStartItemProvider? requestStartItemProvider);
}
