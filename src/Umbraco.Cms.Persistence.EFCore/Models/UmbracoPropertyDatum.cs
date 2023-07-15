namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoPropertyDatum
{
    public int Id { get; set; }

    public int VersionId { get; set; }

    public int PropertyTypeId { get; set; }

    public int? LanguageId { get; set; }

    public string? Segment { get; set; }

    public int? IntValue { get; set; }

    public decimal? DecimalValue { get; set; }

    public DateTime? DateValue { get; set; }

    public string? VarcharValue { get; set; }

    public string? TextValue { get; set; }

    public virtual UmbracoLanguage? Language { get; set; }

    public virtual CmsPropertyType PropertyType { get; set; } = null!;

    public virtual UmbracoContentVersion Version { get; set; } = null!;
}
