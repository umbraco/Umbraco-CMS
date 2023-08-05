namespace Umbraco.Search.Diagnostics;

public interface ISearchEngine
{
    string Name { get; }
    string NativeSyntaxName { get; }
    string Version { get; }
    string NativeSyntaxDocumentationLink { get; }
}
