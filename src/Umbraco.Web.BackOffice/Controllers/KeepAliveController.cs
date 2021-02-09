using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [IsBackOffice]
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
