using Lifti;

namespace Umbraco.Search.InMemory;

public interface ILiftiIndex
{
    public string Name { get; }
    FullTextIndex<string> LiftiIndex { get; }
}

public class UmbracoLiftiIndex : ILiftiIndex
{
    public string Name { get; }
    public FullTextIndex<string> LiftiIndex { get; }

    public UmbracoLiftiIndex(string name, FullTextIndex<string> liftiIndex)
    {
        Name = name;
        LiftiIndex = liftiIndex;
    }
}
