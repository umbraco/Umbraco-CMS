namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class PropertyTypeValidation
{
    public bool Mandatory { get; set; }

    public string? MandatoryMessage { get; set; }

    public string? RegEx { get; set; }

    public string? RegExMessage { get; set; }
}
