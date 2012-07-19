using System.Diagnostics;
using System.Xml;
using umbraco;

namespace Umbraco.Web.Routing
{

    // lookup /<profile>/<login> where <profile> is the profile page and <login> a login
    // this is a faily limited way of doing it, it should better be done with a rewriting
    // rule that would support multiple profile pages for multilingual websites, etc.
    //
    // we're keeping it here only for backward compatibility.
    //
    [LookupWeight(40)]
    internal class LookupByProfile : LookupByPath, ILookup
    {
        private readonly UmbracoContext _umbracoContext;
        static readonly TraceSource Trace = new TraceSource("LookupByProfile");


        public LookupByProfile(ContentStore contentStore, RoutesCache routesCache, UmbracoContext umbracoContext)
            : base(contentStore, routesCache)
        {
            _umbracoContext = umbracoContext;
        }

        public override bool LookupDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

            bool isProfile = false;
            var pos = docreq.Path.LastIndexOf('/');
            if (pos > 0)
            {
                var memberLogin = docreq.Path.Substring(pos + 1);
                var path = docreq.Path.Substring(0, pos);

                if (path == GlobalSettings.ProfileUrl)
                {
                    isProfile = true;
                    Trace.TraceInformation("Path \"{0}\" is the profile path", path);

                    var route = docreq.HasDomain ? (docreq.Domain.RootNodeId.ToString() + path) : path;
                    node = LookupDocumentNode(docreq, route);

                    if (node != null)
                        _umbracoContext.HttpContext.Items["umbMemberLogin"] = memberLogin;
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