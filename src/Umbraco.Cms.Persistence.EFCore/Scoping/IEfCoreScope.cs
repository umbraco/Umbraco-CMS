namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : IDisposable
{
    public IUmbracoEfCoreDatabase UmbracoEfCoreDatabase { get; }

    public void Complete();
}
