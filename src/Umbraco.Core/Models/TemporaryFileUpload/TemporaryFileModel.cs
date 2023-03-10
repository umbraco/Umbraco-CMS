namespace Umbraco.Cms.Core.Models.TemporaryFile;

public class TemporaryFileModel : IDisposable
{
    public Stream DataStream { get; set; } = MemoryStream.Null;
    public string FileName { get; set; } = string.Empty;
    public Guid Key { get; set; }
    public DateTime? AvailableUntil { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private bool _Disposed = false;
    protected void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                DataStream.Dispose();
            }

            // Unmanaged resources are released here.
            _Disposed = true;
        }
    }

    ~TemporaryFileModel()
    {
        Dispose(false);
    }
}
