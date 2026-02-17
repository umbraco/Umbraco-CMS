using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public class AsyncRepositoryBase : IRepository
{

    public AsyncRepositoryBase(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor, AppCaches appCaches)
    {
        ScopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
        AppCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
    }

    /// <summary>
    ///     Gets the <see cref="AppCaches" />
    /// </summary>
    protected AppCaches AppCaches { get; }

    /// <summary>
    ///     Gets the <see cref="IEFCoreScopeAccessor{TDbContext}" />
    /// </summary>
    protected IEFCoreScopeAccessor<UmbracoDbContext> ScopeAccessor { get; }

    /// <summary>
    ///     Gets the AmbientScope
    /// </summary>
    protected IEfCoreScope<UmbracoDbContext> AmbientScope
    {
        get
        {
            IEfCoreScope<UmbracoDbContext>? scope = ScopeAccessor.AmbientScope;
            if (scope == null)
            {
                throw new InvalidOperationException("Cannot run a repository without an ambient scope.");
            }

            return scope;
        }
    }
}
