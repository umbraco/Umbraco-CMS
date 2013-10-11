using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <see cref="UmbracoContext"/>.
    /// </summary>
    public static class UmbracoContextExtensions
    {
        /// <summary>
        /// Informs the context that content has changed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// <para>The contextual caches may, although that is not mandatory, provide an immutable snapshot of
        /// the content over the duration of the context. If you make changes to the content and do want to have
        /// the caches update their snapshot, you have to explicitely ask them to do so by calling ContentHasChanged.</para>
        /// <para>The context informs the contextual caches that content has changed.</para>
        /// </remarks>
        public static void ContentHasChanged(this UmbracoContext context)
        {
            context.ContentCache.ContentHasChanged();
            context.MediaCache.ContentHasChanged();
        }
    }
}
