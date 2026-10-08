// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.Common;

/// <summary>
///     Provides an implementation of <see cref="IPropertyRenderingContextAccessor" /> for tests.
/// </summary>
public class TestPropertyRenderingContextAccessor : IPropertyRenderingContextAccessor
{
    /// <inheritdoc />
    public PropertyRenderingContext PropertyRenderingContext { get; set; }
}
