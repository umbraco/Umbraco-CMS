namespace Umbraco.Cms.Persistence.EFCore;

public interface IUmbracoEfCoreDatabaseFactory : IDisposable
{
    IUmbracoEfCoreDatabase Create();
}
