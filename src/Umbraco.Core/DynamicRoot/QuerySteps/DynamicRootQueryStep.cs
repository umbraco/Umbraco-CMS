namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class DynamicRootQueryStep
{
    /// <summary>
    /// Empty means all Doctypes
    /// </summary>
    public IEnumerable<Guid> AnyOfDocTypeKeys { get; set; } = Array.Empty<Guid>();

    public string Alias { get; set; } = string.Empty;
}
