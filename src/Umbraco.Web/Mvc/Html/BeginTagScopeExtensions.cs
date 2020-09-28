using System.Web.Mvc;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering tag scopes (tags with seperate start/end tags).
    /// </summary>
    public static class BeginTagScopeExtensions
    {
        /// <summary>
        /// Begins the tag scope (only writes the start/end tag when <paramref name="start" /> is set to <c>true</c> or <see cref="IDisposableTagScope.Start" /> is called).
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <param name="start">If set to <c>true</c>, directly writes the start tag.</param>
        /// <returns>
        /// The tag scope, which automatically writes the end tag for an open start tag when disposed.
        /// </returns>
        /// <example>
        /// @using (Html.BeginTagScope("strong", new { @class = "root-node" }, Model.Parent == null)) {}
        ///
        /// @using (var list = Html.BeginTagScope("dl", new { @class = "contact-details" }))
        /// {
        ///     if (!string.IsNullOrEmpty(Model.Telephone))
        ///     {
        ///         list.Start();
        ///
        ///         <dt>Telephone</dt>
        ///         <dd>@Model.Telephone</dd>
        ///     }
        ///
        ///     if (!string.IsNullOrEmpty(Model.Email))
        ///     {
        ///         list.Start();
        ///
        ///         <dt>Email</dt>
        ///         <dd>@Model.Email</dd>
        ///     }
        /// }
        /// </example>
        public static IDisposableTagScope BeginTagScope(this HtmlHelper htmlHelper, string tagName, object htmlAttributes = null, bool start = false)
        {
            var disposableTagScope = new DisposableTagScope(htmlHelper.ViewContext, tagName, htmlAttributes);
            if (start)
            {
                disposableTagScope.Start();
            }

            return disposableTagScope;
        }
    }
}
