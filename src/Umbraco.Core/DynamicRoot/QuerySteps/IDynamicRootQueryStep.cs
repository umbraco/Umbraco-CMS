using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public interface IDynamicRootQueryStep
{
    Task<Attempt<ICollection<Guid>>> ExecuteAsync(ICollection<Guid> origins, DynamicRootQueryStep filter);
}
