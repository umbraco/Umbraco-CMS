namespace Umbraco.Search.Lifti;

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