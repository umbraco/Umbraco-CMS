using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a CurrentUICulture-based implementation of <see cref="IVariationContextAccessor"/>.
    /// </summary>
    /// <remarks>
    /// <para>This accessor does not support segments. There is no need to set the current context.</para>
    /// </remarks>
    public class ThreadCultureVariationContextAccessor : IVariationContextAccessor
    {
        private readonly ConcurrentDictionary<string, VariationContext> _contexts = new ConcurrentDictionary<string, VariationContext>();

        public VariationContext VariationContext
        {
            get => _contexts.GetOrAdd(Thread.CurrentThread.CurrentUICulture.Name, culture => new VariationContext(culture));
            set => throw new NotSupportedException();
        }
    }
}
