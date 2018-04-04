using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles profiles.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/profile/login</c> where <c>/profile</c> is the profile page nice url and <c>login</c> the login of a member.</para>
    /// <para>This should rather be done with a rewriting rule. There would be multiple profile pages in multi-sites/multi-langs setups.
    /// We keep it for backward compatility reasons.</para>
    /// </remarks>
    public class ContentFinderByProfile : ContentFinderByNiceUrl
    {
        public ContentFinderByProfile(ILogger logger)
            : base(logger)
        { }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public override bool TryFindContent(PublishedRequest frequest)
        {
            IPublishedContent node = null;
            var path = frequest.Uri.GetAbsolutePathDecoded();

            var isProfile = false;
            var pos = path.LastIndexOf('/');
            if (pos > 0)
            {
                var memberLogin = path.Substring(pos + 1);
                path = path.Substring(0, pos);

                if (path == GlobalSettings.ProfileUrl)
                {
                    isProfile = true;
                    Logger.Debug<ContentFinderByProfile>(() => $"Path \"{path}\" is the profile path");

                    var route = frequest.HasDomain ? (frequest.Domain.ContentId + path) : path;
                    node = FindContent(frequest, route);

                    if (node != null)
                    {
                        //TODO: Should be handled by Context Items class manager (http://issues.umbraco.org/issue/U4-61)
                        frequest.UmbracoContext.HttpContext.Items["umbMemberLogin"] = memberLogin;
                    }
                    else
                    {
                        Logger.Debug<ContentFinderByProfile>("No document matching profile path?");
                    }
                }
            }

            if (isProfile == false)
            {
                Logger.Debug<ContentFinderByProfile>("Not the profile path");
            }

            return node != null;
        }
    }
}
