namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Specifies the lifetime of a registered instance.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// Always get a new instance.
        /// </summary>
        /// <remarks>Corresponds to Transient in LightInject, Castle Windsor
        /// or MS.DI, PerDependency in Autofac.</remarks>
        Transient,

        /// <summary>
        /// One unique instance per request.
        /// </summary>
        // fixme - not what you think!
        // currently, corresponds to 'Request' in LightInject which is 'Transient + disposed by Scope'
        // but NOT (in LightInject) a per-web-request lifetime, more a TransientScoped
        //
        // we use it for controllers, httpContextBase and umbracoContext
        // - so that they are automatically disposed at the end of the scope (ie request)
        // - not sure they should not be simply 'scoped'?
        //
        // Castle has an extra PerWebRequest something, and others use scope
        // what about Request before first request ie during application startup?
        // see http://blog.ploeh.dk/2009/11/17/UsingCastleWindsor'sPerWebRequestlifestylewithASP.NETMVConIIS7/
        // Castle ends up requiring a special scope manager too
        // see https://groups.google.com/forum/#!topic/castle-project-users/1E2W9LVIYR4
        //
        // but maybe also - why are we requiring scoped services at startup?
        Request,

        /// <summary>
        /// One unique instance per container scope.
        /// </summary>
        /// <remarks>Corresponds to Scope in LightInject, Scoped in MS.DI
        /// or Castle Windsor, PerLifetimeScope in Autofac.</remarks>
        Scope,

        /// <summary>
        /// One unique instance per container.
        /// </summary>
        /// <remarks>Corresponds to Singleton in LightInject, Castle Windsor
        /// or MS.DI and to SingleInstance in Autofac.</remarks>
        Singleton
    }
}
