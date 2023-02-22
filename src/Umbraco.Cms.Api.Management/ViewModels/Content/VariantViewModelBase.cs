namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class VariantViewModelBase
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }
}
