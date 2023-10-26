namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class DynamicRootQueryStep
{
    public IEnumerable<Guid> AnyOfDocTypeKeys { get; set; } = Array.Empty<Guid>(); // empty is all / *
    public string Alias { get; set; } = string.Empty;
}
