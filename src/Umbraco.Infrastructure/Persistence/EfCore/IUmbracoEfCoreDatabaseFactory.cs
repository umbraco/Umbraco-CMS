namespace Umbraco.Cms.Infrastructure.Persistence.EfCore;

public interface IUmbracoEfCoreDatabaseFactory
{
    IUmbracoEfCoreDatabase Create();
}
