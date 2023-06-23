namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class TemporaryFileResponseModel
{
    public Guid Id { get; set; }

    public DateTime? AvailableUntil { get; set; }

    public string FileName { get; set; } = string.Empty;
}
