namespace Umbraco.Cms.Core.Models.ContentEditing;

public class PropertyValueModel
{
    public required string Alias { get; set; }

    public required object? Value { get; set; }
}
