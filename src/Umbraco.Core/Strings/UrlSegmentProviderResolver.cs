﻿using System;
using System.Collections.Generic;
﻿using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Resolves IUrlSegmentProvider objects.
    /// </summary>
    public sealed class UrlSegmentProviderResolver : ManyObjectsResolverBase<UrlSegmentProviderResolver, IUrlSegmentProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSegmentProviderResolver"/> class with an initial list of provider types.
        /// </summary>
        /// <param name="providerTypes">The list of provider types.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal UrlSegmentProviderResolver(IEnumerable<Type> providerTypes)
            : base(providerTypes)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSegmentProviderResolver"/> class with an initial list of provider types.
        /// </summary>
        /// <param name="providerTypes">The list of provider types.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal UrlSegmentProviderResolver(params Type[] providerTypes)
            : base(providerTypes)
        { }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        public IEnumerable<IUrlSegmentProvider> Providers
        {
            get { return Values; }
        }
    }
}