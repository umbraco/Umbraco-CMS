namespace Umbraco.Cms.Core.StartNodeFinder;

public interface IStartNodeFinder
{
    IEnumerable<Guid> GetDynamicStartNodes(StartNodeSelector startNodeSelector);
}
