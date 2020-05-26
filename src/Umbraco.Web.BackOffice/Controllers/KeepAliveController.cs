using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Web.BackOffice.Controllers
{
    public class KeepAliveController : UmbracoApiController
    {
        [OnlyLocalRequests]
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
