namespace Umbraco.Search.InMemory;

public interface ILiftiIndexManager
{
    ILiftiIndex? GetIndex(string name);
}