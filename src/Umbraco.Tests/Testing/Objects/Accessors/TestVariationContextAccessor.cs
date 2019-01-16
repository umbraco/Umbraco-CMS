using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    /// <summary>
    /// Provides an implementation of <see cref="IVariationContextAccessor"/> for tests.
    /// </summary>
    public class TestVariationContextAccessor : IVariationContextAccessor
    {
        /// <inheritdoc />
        public VariationContext VariationContext
        {
            get;
            set;
        } = new VariationContext();
    }
}
