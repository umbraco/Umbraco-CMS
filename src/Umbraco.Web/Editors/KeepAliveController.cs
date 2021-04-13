using System.Runtime.Serialization;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    public class KeepAliveController : UmbracoApiController
    {
        [HttpHead]
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
