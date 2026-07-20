using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi.Accessors;

/// <summary>
///     A no-operation implementation of <see cref="IRequestStartItemProviderAccessor"/> that always returns a <see cref="NoopRequestStartItemProvider"/>.
/// </summary>
public sealed class NoopRequestStartItemProviderAccessor : IRequestStartItemProviderAccessor
{
    /// <inheritdoc />
    public bool TryGetValue([NotNullWhen(true)] out IRequestStartItemProvider? requestStartItemProvider)
    {
        requestStartItemProvider = new NoopRequestStartItemProvider();
        return true;
    }
}
