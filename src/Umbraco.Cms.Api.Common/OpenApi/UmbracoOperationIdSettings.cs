namespace Umbraco.Cms.Api.Common.OpenApi;

public class UmbracoOperationIdSettings
{
    private HashSet<string> _nameSpacePrefixes = new HashSet<string>()
    {
        "Umbraco.Cms.Api"
    };

    public IReadOnlySet<string> NameSpacePrefixes
    {
        get => _nameSpacePrefixes;
    }

    public bool AddNameSpacePrefix(string prefix) => _nameSpacePrefixes.Add(prefix);
}
