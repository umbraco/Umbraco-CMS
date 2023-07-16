namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class CmsTag
{
    public int Id { get; set; }

    public string Group { get; set; } = null!;

    public int? LanguageId { get; set; }

    public string Tag { get; set; } = null!;

    public virtual ICollection<CmsTagRelationship> CmsTagRelationships { get; set; } = new List<CmsTagRelationship>();

    public virtual UmbracoLanguage? Language { get; set; }
}
