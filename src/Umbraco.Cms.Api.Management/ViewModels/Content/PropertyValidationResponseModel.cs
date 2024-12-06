namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public class PropertyValidationResponseModel
{
    public string[] Messages { get; set; } = Array.Empty<string>();

    public string Alias { get; set; } = string.Empty;

    public string? Culture { get; set; }

    public string? Segment { get; set; }
}
