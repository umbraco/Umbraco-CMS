﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
﻿using Umbraco.Core.Logging;
﻿using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Resolves IUrlProvider objects.
    /// </summary>
    public sealed class UrlProviderResolver : ManyObjectsResolverBase<UrlProviderResolver, IUrlProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProviderResolver"/> class with an initial list of provider types.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="providerTypes">The list of provider types.</param>
        /// <param name="serviceProvider"></param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal UrlProviderResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> providerTypes)
            : base(serviceProvider, logger, providerTypes)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProviderResolver"/> class with an initial list of provider types.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="providerTypes">The list of provider types.</param>
        /// <param name="serviceProvider"></param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal UrlProviderResolver(IServiceProvider serviceProvider, ILogger logger, params Type[] providerTypes)
            : base(serviceProvider, logger, providerTypes)
        { }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        public IEnumerable<IUrlProvider> Providers
        {
            get { return Values; }
        }
    }
}