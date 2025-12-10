namespace Umbraco.Cms.Core.Models;

public sealed class EntityDataPickerValue
{
    public required IEnumerable<string> Ids { get; set; }

    public required string DataSource { get; set; }
}
