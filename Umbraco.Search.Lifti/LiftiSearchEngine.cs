using Umbraco.Search.Diagnostics;

namespace Umbraco.Search.Lifti;

public class LiftiSearchEngine : ISearchEngine
{
    public string Name { get; } = "LIFTI";
    public string NativeSyntaxName { get; } = "The LIFTI Query Syntax";
    public string Version { get; } = "5.0.0";

    public string NativeSyntaxDocumentationLink { get; } =
        "https://mikegoatly.github.io/lifti/docs/searching/";
}
