namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsPropertyType
{
    public int Id { get; set; }

    public int DataTypeId { get; set; }

    public int ContentTypeId { get; set; }

    public int? PropertyTypeGroupId { get; set; }

    public string Alias { get; set; } = null!;

    public string? Name { get; set; }

    public int SortOrder { get; set; }

    public bool? Mandatory { get; set; }

    public string? MandatoryMessage { get; set; }

    public string? ValidationRegExp { get; set; }

    public string? ValidationRegExpMessage { get; set; }

    public string? Description { get; set; }

    public bool? LabelOnTop { get; set; }

    public int Variations { get; set; }

    public Guid UniqueId { get; set; }

    public virtual ICollection<CmsTagRelationship> CmsTagRelationships { get; set; } = new List<CmsTagRelationship>();

    public virtual CmsContentType ContentType { get; set; } = null!;

    public virtual UmbracoDataType DataType { get; set; } = null!;

    public virtual CmsPropertyTypeGroup? PropertyTypeGroup { get; set; }

    public virtual ICollection<UmbracoPropertyDatum> UmbracoPropertyData { get; set; } = new List<UmbracoPropertyDatum>();
}
