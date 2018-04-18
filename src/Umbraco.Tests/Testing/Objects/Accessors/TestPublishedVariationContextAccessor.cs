using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    /// <summary>
    /// Provides an implementation of <see cref="IPublishedVariationContextAccessor"/> for tests.
    /// </summary>
    public class TestPublishedVariationContextAccessor : IPublishedVariationContextAccessor
    {
        /// <inheritdoc />
        public PublishedVariationContext Context { get; set; }
    }
}
