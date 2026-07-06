namespace Umbraco.Cms.Search.Provider.Examine.Services;

public interface IIndexCommitMonitor
{
    Task<bool> WaitForCommitAsync(string indexAlias, CancellationToken cancellationToken);
}
