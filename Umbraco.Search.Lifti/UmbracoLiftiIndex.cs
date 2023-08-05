using Lifti;

namespace Umbraco.Search.Lifti;

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