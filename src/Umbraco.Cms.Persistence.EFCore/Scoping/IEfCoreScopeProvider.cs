namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScopeProvider
{
    IEfCoreScope CreateScope();

    void PopAmbientScope();
}
