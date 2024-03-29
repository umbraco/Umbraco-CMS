using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class VariantResponseModelBase : VariantModelBase
{
    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset UpdateDate { get; set; }
}
