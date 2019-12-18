using System.Web;

namespace Umbraco.Web
{
    /// <summary>
    /// Creates and manages <see cref="UmbracoContext"/> instances.
    /// </summary>
    public interface IUmbracoContextFactory
    {
        /// <summary>
        /// Ensures that a current <see cref="UmbracoContext"/> exists.
        /// </summary>
        /// <remarks>
        /// <para>If an <see cref="UmbracoContext"/> is already registered in the
        /// <see cref="IUmbracoContextAccessor"/>, returns a non-root reference to it.
        /// Otherwise, create a new instance, registers it, and return a root reference
        /// to it.</para>
        /// <para>If <paramref name="httpContext"/> is null, the factory tries to use
        /// <see cref="HttpContext.Current"/> if it exists. Otherwise, it uses a dummy
        /// <see cref="HttpContextBase"/>.</para>
        /// </remarks>
        /// <example>
        /// using (var contextReference = contextFactory.EnsureUmbracoContext())
        /// {
        ///   var umbracoContext = contextReference.UmbracoContext;
        ///   // use umbracoContext...
        /// }
        /// </example>
        /// <param name="httpContext">An optional http context.</param>
        UmbracoContextReference EnsureUmbracoContext(HttpContextBase httpContext = null);
    }
}