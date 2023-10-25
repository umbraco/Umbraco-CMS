namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class DynamicRootQueryStep
{
    public IEnumerable<string> AnyOfDocTypeAlias { get; set; } = Array.Empty<string>(); // empty is all / *
    public string Alias { get; set; } = string.Empty;
}
