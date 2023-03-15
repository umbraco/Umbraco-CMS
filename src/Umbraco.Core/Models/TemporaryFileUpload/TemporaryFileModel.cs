namespace Umbraco.Cms.Core.Models.TemporaryFile;

public class TemporaryFileModel : TemporaryFileModelBase
{
    public required DateTime AvailableUntil { get; set; }
}
