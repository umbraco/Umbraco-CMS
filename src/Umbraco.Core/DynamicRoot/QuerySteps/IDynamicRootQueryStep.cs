using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public interface IDynamicRootQueryStep
{
    bool Execute(IEnumerable<Guid> origins, DynamicRootQueryStep filter, [MaybeNullWhen(false)]out IEnumerable<Guid> result);
}
