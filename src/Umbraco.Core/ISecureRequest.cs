using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Core
{
    /// <summary>
    /// Service to encapsulate if a request is executing under HTTPS
    /// </summary>
    public interface ISecureRequest
    {
        bool IsSecure(HttpRequestBase httpRequest);
        string GetScheme(HttpRequestBase request);
    }
}
