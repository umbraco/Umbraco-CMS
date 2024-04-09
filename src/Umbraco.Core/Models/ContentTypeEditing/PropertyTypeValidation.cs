namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class PropertyTypeValidation
{
    public bool Mandatory { get; set; }

    public string? MandatoryMessage { get; set; }

    public string? RegularExpression { get; set; }

    public string? RegularExpressionMessage { get; set; }
}
