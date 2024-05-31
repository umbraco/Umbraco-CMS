namespace Umbraco.Cms.Core.Models.TemporaryFile;

public class TemporaryFileModel : TemporaryFileModelBase
{
    public Guid Key { get; set; }
    public required DateTime AvailableUntil { get; set; }
}
