namespace Umbraco.Search.Lifti;

public interface ILiftiIndexManager
{
    ILiftiIndex? GetIndex(string name);
}