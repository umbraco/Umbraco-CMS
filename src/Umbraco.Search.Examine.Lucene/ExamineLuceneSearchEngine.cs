using Umbraco.Search.Diagnostics;

namespace Umbraco.Search.Examine.Lucene;

public class ExamineLuceneSearchEngine : ISearchEngine
{
    public string Name { get; } = "Examine.Lucene";
    public string NativeSyntaxName { get; } = "Lucene";
    public string Version { get; } = "4.8.0";

    public string NativeSyntaxDocumentationLink { get; } =
        "https://lucene.apache.org/core/4_8_0/queryparser/index.html?org/apache/lucene/queryparser/classic/QueryParser.html";
}
