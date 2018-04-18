using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a CurrentUICulture-based implementation of <see cref="IPublishedVariationContextAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>This accessor does not support segments. There is no need to set the current context.</para>
    /// </remarks>
    public class ThreadCulturePublishedVariationContextAccessor : IPublishedVariationContextAccessor
    {
        private readonly ConcurrentDictionary<string, PublishedVariationContext> _contexts = new ConcurrentDictionary<string, PublishedVariationContext>();

        public PublishedVariationContext Context
        {
            get => _contexts.GetOrAdd(Thread.CurrentThread.CurrentUICulture.Name, culture => new PublishedVariationContext { Culture = culture });
            set => throw new NotSupportedException();
        }
    }
}