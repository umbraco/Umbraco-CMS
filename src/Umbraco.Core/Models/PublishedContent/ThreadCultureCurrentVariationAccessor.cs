using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a CurrentUICulture-based implementation of <see cref="ICurrentVariationAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>This accessor does not support segments. There is no need to set the current context.</para>
    /// </remarks>
    public class ThreadCultureCurrentVariationAccessor : ICurrentVariationAccessor
    {
        private readonly ConcurrentDictionary<string, CurrentVariation> _contexts = new ConcurrentDictionary<string, CurrentVariation>();

        public CurrentVariation CurrentVariation
        {
            get => _contexts.GetOrAdd(Thread.CurrentThread.CurrentUICulture.Name, culture => new CurrentVariation { Culture = culture });
            set => throw new NotSupportedException();
        }
    }
}