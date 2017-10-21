using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.TokenReplacers
{
    /// <summary>
    /// Resolves all token replacer instances
    /// </summary>
    /// <remarks>
    /// Each instance scoped to the lifespan of the http request
    /// </remarks>
    internal class TokenReplacerResolver : LazyManyObjectsResolverBase<TokenReplacerResolver, ITokenReplacer>, ITokenReplacerResolver
    {
        public TokenReplacerResolver(ILogger logger, Func<IEnumerable<Type>> lazyTypeList)
            : base(new TokenReplacerServiceProvider(), logger, lazyTypeList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns all health check notification method instances
        /// </summary>
        public IEnumerable<ITokenReplacer> TokenReplacers
        {
            get { return Values; }
        }

        /// <summary>
        /// This will ctor the ITokenReplacerResolver instances
        /// </summary>
        private class TokenReplacerServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                var normalArgs = new[] { typeof(TokenReplacerContext) };
                var found = serviceType.GetConstructor(normalArgs);
                if (found != null)
                {
                    var tokenReplacerContext = new TokenReplacerContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current);
                    return found.Invoke(new object[]
                        {
                            tokenReplacerContext
                        });
                }

                return Activator.CreateInstance(serviceType);
            }
        }
    }
}
