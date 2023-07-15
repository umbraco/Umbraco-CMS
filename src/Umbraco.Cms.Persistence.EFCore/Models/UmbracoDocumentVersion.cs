namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoDocumentVersion
{
    public int Id { get; set; }

    public int? TemplateId { get; set; }

    public bool Published { get; set; }

    public virtual UmbracoContentVersion IdNavigation { get; set; } = null!;

    public virtual CmsTemplate? Template { get; set; }
}
