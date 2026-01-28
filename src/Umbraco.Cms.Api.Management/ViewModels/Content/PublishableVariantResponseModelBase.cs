using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class PublishableVariantResponseModelBase : VariantResponseModelBase
{
    public DocumentVariantState State { get; set; }

    public DateTimeOffset? PublishDate { get; set; }

    public DateTimeOffset? ScheduledPublishDate { get; set; }

    public DateTimeOffset? ScheduledUnpublishDate { get; set; }
}
