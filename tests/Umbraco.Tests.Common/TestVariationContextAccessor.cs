// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.Common;

/// <summary>
///     Provides an implementation of <see cref="IVariationContextAccessor" /> for tests.
/// </summary>
public class TestVariationContextAccessor : IVariationContextAccessor
{
    /// <inheritdoc />
    public VariationContext VariationContext { get; set; }
}
