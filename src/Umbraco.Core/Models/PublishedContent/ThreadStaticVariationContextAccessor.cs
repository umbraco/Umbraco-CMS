using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a ThreadStatic-based implementation of <see cref="IVariationContextAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>Something must set the current context.</para>
    /// </remarks>
    public class ThreadStaticVariationContextAccessor : IVariationContextAccessor
    {
        [ThreadStatic]
        private static VariationContext _context;

        /// <inheritdoc />
        public VariationContext VariationContext
        {
            get => _context;
            set => _context = value;
        }
    }
}