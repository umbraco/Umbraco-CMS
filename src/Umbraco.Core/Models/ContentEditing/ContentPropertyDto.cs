namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content property from the database
/// </summary>
public class ContentPropertyDto : ContentPropertyBasic
{
    public IDataType? DataType { get; set; }

    public string? Label { get; set; }

    public string? Description { get; set; }

    public bool? IsRequired { get; set; }

    public bool? LabelOnTop { get; set; }

    public string? IsRequiredMessage { get; set; }

    public string? ValidationRegExp { get; set; }

    public string? ValidationRegExpMessage { get; set; }
}
