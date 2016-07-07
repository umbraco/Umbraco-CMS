using System;
using System.Linq.Expressions;
using LightInject;
using umbraco.cms.presentation;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Resolves the <see cref="ISiteDomainHelper"/> implementation.
    /// </summary>
    public sealed class SiteDomainHelperResolver : ContainerSingleObjectResolver<SiteDomainHelperResolver, ISiteDomainHelper>
    {
        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        internal SiteDomainHelperResolver(IServiceContainer container)
            : base(container)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDomainHelperResolver"/> class with an <see cref="ISiteDomainHelper"/> implementation.
        /// </summary>
        /// <param name="helper">The <see cref="ISiteDomainHelper"/> implementation.</param>
        internal SiteDomainHelperResolver(ISiteDomainHelper helper)
            : base(helper)
        { }


        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationType"></param>
        internal SiteDomainHelperResolver(IServiceContainer container, Func<IServiceFactory, ISiteDomainHelper> implementationType)
            : base(container, implementationType)
        {
        }

        /// <summary>
        /// Can be used by developers at runtime to set their IDomainHelper at app startup
        /// </summary>
        /// <param name="helper"></param>
        public void SetHelper(ISiteDomainHelper helper)
        {
            Value = helper;
        }

        /// <summary>
        /// Gets or sets the <see cref="ISiteDomainHelper"/> implementation.
        /// </summary>
        public ISiteDomainHelper Helper
        {
            get { return Value; }
        }
    }
}