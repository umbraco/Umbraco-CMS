namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoContentVersion
{
    public int Id { get; set; }

    public int NodeId { get; set; }

    public DateTime VersionDate { get; set; }

    public int? UserId { get; set; }

    public bool Current { get; set; }

    public string? Text { get; set; }

    public bool? PreventCleanup { get; set; }

    public virtual UmbracoContent Node { get; set; } = null!;

    public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; } = new List<UmbracoContentVersionCultureVariation>();

    public virtual UmbracoDocumentVersion? UmbracoDocumentVersion { get; set; }

    public virtual UmbracoMediaVersion? UmbracoMediaVersion { get; set; }

    public virtual ICollection<UmbracoPropertyDatum> UmbracoPropertyData { get; set; } = new List<UmbracoPropertyDatum>();

    public virtual UmbracoUser? User { get; set; }
}
