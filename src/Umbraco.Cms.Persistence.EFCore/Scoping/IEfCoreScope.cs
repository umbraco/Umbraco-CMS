namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope
{
    public IUmbracoEfCoreDatabase UmbracoEfCoreDatabase { get; }

    public void Complete();
}
