using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;

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
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public override bool TryFindContent(PublishedContentRequest docRequest)
        {
            IPublishedContent node = null;
			var path = docRequest.Uri.GetAbsolutePathDecoded();

            bool isProfile = false;
			var pos = path.LastIndexOf('/');
            if (pos > 0)
            {
				var memberLogin = path.Substring(pos + 1);
				path = path.Substring(0, pos);

                if (path == GlobalSettings.ProfileUrl)
                {
                    isProfile = true;
					LogHelper.Debug<ContentFinderByProfile>("Path \"{0}\" is the profile path", () => path);

					var route = docRequest.HasDomain ? (docRequest.Domain.RootNodeId.ToString() + path) : path;
					node = FindContent(docRequest, route);

                    if (node != null)
                    {
						//TODO: Should be handled by Context Items class manager (http://issues.umbraco.org/issue/U4-61)
						docRequest.RoutingContext.UmbracoContext.HttpContext.Items["umbMemberLogin"] = memberLogin;	
                    }                        
                    else
                    {
						LogHelper.Debug<ContentFinderByProfile>("No document matching profile path?");
                    }
                }
            }

            if (!isProfile)
            {
				LogHelper.Debug<ContentFinderByProfile>("Not the profile path");
            }

            return node != null;
        }
    }
}