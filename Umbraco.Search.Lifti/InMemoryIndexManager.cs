using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Search.InMemory;

public class LiftiIndexManager : ILiftiIndexManager
{
    private readonly IEnumerable<ILiftiIndex?> _liftiIndices;

    public LiftiIndexManager(IEnumerable<ILiftiIndex?> liftiIndices)
    {
        _liftiIndices = liftiIndices;
    }

    public ILiftiIndex? GetIndex(string name)
    {
        return _liftiIndices.FirstOrDefault(x => x?.Name == name);
    }
}

public interface ILiftiIndexManager
{
    ILiftiIndex? GetIndex(string name);
}
