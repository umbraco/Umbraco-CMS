namespace Umbraco.Cms.Core.Web;

/// <summary>
///     Creates and manages <see cref="IUmbracoContext" /> instances.
/// </summary>
public interface IUmbracoContextFactory
{
    /// <summary>
    ///     Ensures that a current <see cref="IUmbracoContext" /> exists.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an <see cref="IUmbracoContext" /> is already registered in the
    ///         <see cref="IUmbracoContextAccessor" />, returns a non-root reference to it.
    ///         Otherwise, create a new instance, registers it, and return a root reference
    ///         to it.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     using (var contextReference = contextFactory.EnsureUmbracoContext())
    ///     {
    ///     var umbracoContext = contextReference.UmbracoContext;
    ///     // use umbracoContext...
    ///     }
    /// </example>
    UmbracoContextReference EnsureUmbracoContext();
}
