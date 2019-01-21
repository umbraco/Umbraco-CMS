using System.Runtime.Serialization;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    // fixme/task - deal with this
    // this is not authenticated, and therefore public, and therefore reveals we
    // are running Umbraco - but, all requests should come from localhost really,
    // so there should be a way to 404 when the request comes from the outside.

    public class KeepAliveController : UmbracoApiController
    {
        [HttpGet]
        public KeepAlivePingResult Ping()
        {
            return new KeepAlivePingResult
            {
                Success = true,
                Message = "I'm alive!"
            };
        }
    }

    public class KeepAlivePingResult
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
