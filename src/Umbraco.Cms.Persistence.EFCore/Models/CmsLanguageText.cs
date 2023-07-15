namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsLanguageText
{
    public int Pk { get; set; }

    public int LanguageId { get; set; }

    public Guid UniqueId { get; set; }

    public string Value { get; set; } = null!;

    public virtual UmbracoLanguage Language { get; set; } = null!;

    public virtual CmsDictionary Unique { get; set; } = null!;
}
