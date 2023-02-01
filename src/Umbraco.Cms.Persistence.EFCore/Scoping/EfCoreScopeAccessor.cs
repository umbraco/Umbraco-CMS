namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class EfCoreScopeAccessor : IEFCoreScopeAccessor
{
    public EfCoreScopeAccessor()
    {

    }
    public IEfCoreScope? AmbientScope { get; }
}
