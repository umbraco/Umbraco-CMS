namespace Umbraco.Cms.Core.Models;

public class Segment
{
    public required string Name { get; set; }

    public required string Alias { get; set; }

    [Obsolete("This property is temporary and will be removed in a future release (planned for v20). A more permanent solution will follow.")]
    public IEnumerable<string>? Cultures { get; set; } = null;
}
