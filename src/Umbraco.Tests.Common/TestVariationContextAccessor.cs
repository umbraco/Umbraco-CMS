// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Common
{
    /// <summary>
    /// Provides an implementation of <see cref="IVariationContextAccessor"/> for tests.
    /// </summary>
    public class TestVariationContextAccessor : IVariationContextAccessor
    {
        /// <inheritdoc />
        public VariationContext VariationContext { get; set; }
    }
}
