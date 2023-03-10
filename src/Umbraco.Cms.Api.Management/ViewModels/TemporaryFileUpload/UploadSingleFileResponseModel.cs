namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class UploadSingleFileResponseModel
{
    public Guid Key { get; set; }

    public DateTime? AvailableUntil { get; set; }
    public string FileName { get; set; } = "";
}
