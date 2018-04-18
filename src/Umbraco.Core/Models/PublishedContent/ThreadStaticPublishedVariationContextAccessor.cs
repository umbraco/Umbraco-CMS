using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a ThreadStatic-based implementation of <see cref="IPublishedVariationContextAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>Something must set the current context.</para>
    /// </remarks>
    public class ThreadStaticPublishedVariationContextAccessor : IPublishedVariationContextAccessor
    {
        [ThreadStatic]
        private static PublishedVariationContext _context;

        /// <inheritdoc />
        public PublishedVariationContext Context
        {
            get => _context;
            set => _context = value;
        }
    }
}