namespace Umbraco.Cms.Core.Models.TemporaryFile;

public class TempFileModel
{
    public Stream DataStream { get; set; } = MemoryStream.Null;
    public string FileName { get; set; } = string.Empty;
    public Guid Key { get; set; }
    public DateTime? AvailableUntil { get; set; }
}
