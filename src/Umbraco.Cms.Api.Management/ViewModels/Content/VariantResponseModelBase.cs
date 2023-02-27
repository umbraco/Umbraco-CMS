namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class VariantResponseModelBase : VariantModelBase
{
    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }
}
