namespace Umbraco.Cms.Core.Models.TemporaryFile;

public abstract class TemporaryFileModelBase
{
    public required string FileName { get; set; }

    public Guid Key { get; set; }

    public Func<Stream> OpenReadStream { get; set; } = () => Stream.Null;
}
