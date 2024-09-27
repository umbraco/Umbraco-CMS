namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentNotificationResponseModel
{
    public required string ActionId { get; set; }

    public required string Alias { get; set; }

    public required bool Subscribed { get; set; }
}
