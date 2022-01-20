﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Scoping
{
    /// <summary>
    /// Provides access to the ambient scope.
    /// </summary>
    public interface IScopeAccessor
    {
        /// <summary>
        /// Gets the ambient scope.
        /// </summary>
        /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
        IScope AmbientScope { get; }
    }
}
