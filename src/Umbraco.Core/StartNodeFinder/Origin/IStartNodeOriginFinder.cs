namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public interface IStartNodeOriginFinder
{
    Guid? FindOriginKey(StartNodeSelector startNodeSelector);
}
