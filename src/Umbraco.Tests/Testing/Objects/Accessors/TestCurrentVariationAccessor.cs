using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    /// <summary>
    /// Provides an implementation of <see cref="ICurrentVariationAccessor"/> for tests.
    /// </summary>
    public class TestCurrentVariationAccessor : ICurrentVariationAccessor
    {
        /// <inheritdoc />
        public CurrentVariation CurrentVariation
        {
            get;
            set;
        }
    }
}
