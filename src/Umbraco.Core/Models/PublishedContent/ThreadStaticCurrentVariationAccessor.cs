using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a ThreadStatic-based implementation of <see cref="ICurrentVariationAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>Something must set the current context.</para>
    /// </remarks>
    public class ThreadStaticCurrentVariationAccessor : ICurrentVariationAccessor
    {
        [ThreadStatic]
        private static CurrentVariation _context;

        /// <inheritdoc />
        public CurrentVariation CurrentVariation
        {
            get => _context;
            set => _context = value;
        }
    }
}