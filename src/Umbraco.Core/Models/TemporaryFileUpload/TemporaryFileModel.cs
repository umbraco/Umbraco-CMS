namespace Umbraco.Cms.Core.Models.TemporaryFile;

public class TemporaryFileModel
{
    public string FileName { get; set; } = string.Empty;
    public Guid Key { get; set; }
    public DateTime? AvailableUntil { get; set; }

    public Func<Stream> OpenReadStream { get; set; } = () => Stream.Null;
}
