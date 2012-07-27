using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Resolving;
using umbraco;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles profiles.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/profile/login</c> where <c>/profile</c> is the profile page nice url and <c>login</c> the login of a member.</para>
	/// <para>This should rather be done with a rewriting rule. There would be multiple profile pages in multi-sites/multi-langs setups.
	/// We keep it for backward compatility reasons.</para>
	/// </remarks>
	[ResolutionWeight(40)]
    internal class LookupByProfile : LookupByNiceUrl, IDocumentLookup
    {
		static readonly TraceSource Trace = new TraceSource("LookupByProfile");

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public override bool TrySetDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

            bool isProfile = false;
			var pos = docreq.Uri.AbsolutePath.LastIndexOf('/');
            if (pos > 0)
            {
				var memberLogin = docreq.Uri.AbsolutePath.Substring(pos + 1);
				var path = docreq.Uri.AbsolutePath.Substring(0, pos);

                if (path == GlobalSettings.ProfileUrl)
                {
                    isProfile = true;
                    Trace.TraceInformation("Path \"{0}\" is the profile path", path);

                    var route = docreq.HasDomain ? (docreq.Domain.RootNodeId.ToString() + path) : path;
                    node = LookupDocumentNode(docreq, route);

                    if (node != null)
                    {
						//TODO: Should be handled by Context Items class manager (http://issues.umbraco.org/issue/U4-61)
						docreq.RoutingContext.UmbracoContext.HttpContext.Items["umbMemberLogin"] = memberLogin;	
                    }                        
                    else
                        Trace.TraceInformation("No document matching profile path?");
                }
            }

            if (!isProfile)
                Trace.TraceInformation("Not the profile path");

            return node != null;
        }
    }
}