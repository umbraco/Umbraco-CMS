using Lifti;

namespace Umbraco.Search.Lifti;

public interface ILiftiIndex
{
    public string Name { get; }
    FullTextIndex<string> LiftiIndex { get; }
}