using Lifti;

namespace Umbraco.Search.InMemory;

public interface ILiftiIndex
{
    public string Name { get; }
    FullTextIndex<string> LiftiIndex { get; }
}