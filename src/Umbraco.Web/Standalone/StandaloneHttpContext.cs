using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Web.Standalone
{
    /// <summary>
    /// An Http context for use in standalone applications.
    /// </summary>
    internal class StandaloneHttpContext : HttpContextBase
    {
        // fixme - what shall we implement here?

        public override HttpRequestBase Request
        {
            get { return null; }
        }
    }
}
