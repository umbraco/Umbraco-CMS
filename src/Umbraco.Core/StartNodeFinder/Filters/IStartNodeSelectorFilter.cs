using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public interface IStartNodeSelectorFilter
{
    bool Filter(IEnumerable<Guid> origins, StartNodeFilter filter, [MaybeNullWhen(false)]out IEnumerable<Guid> result);
}
