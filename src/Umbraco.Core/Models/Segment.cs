namespace Umbraco.Cms.Core.Models;

public class Segment
{
    public required string Name { get; set; }

    public required string Alias { get; set; }

    public IEnumerable<string>? Cultures { get; set; } = null;
}
