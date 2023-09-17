namespace Umbraco.Search.Diagnostics;

public interface ISearchEngine
{
    string Name { get; }
    string NativeSyntaxName { get; }
    string Version { get; }
    string NativeSyntaxDocumentationLink { get; }

    public IReadOnlyDictionary<string, object?>? ToDictionary()
    {
        return new Dictionary<string, object?>()
        {
            {nameof(Name), Name},
            {nameof(NativeSyntaxName), NativeSyntaxName},
            {nameof(Version), Version},
            {nameof(NativeSyntaxDocumentationLink), NativeSyntaxDocumentationLink}
        };
}
}
