namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoLogViewerQuery
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Query { get; set; } = null!;
}
