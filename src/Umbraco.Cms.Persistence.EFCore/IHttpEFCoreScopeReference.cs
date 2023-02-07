namespace Umbraco.Cms.Persistence.EFCore;

public interface IHttpEFCoreScopeReference : IDisposable
{
    /// <summary>
    ///     Register for cleanup in the request
    /// </summary>
    void Register();
}
