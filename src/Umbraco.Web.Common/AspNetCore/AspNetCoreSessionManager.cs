﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore
{
    internal class AspNetCoreSessionManager : ISessionIdResolver, ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreSessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// If session isn't enabled this will throw an exception so we check
        /// </summary>
        private bool IsSessionsAvailable => !(_httpContextAccessor.HttpContext?.Features.Get<ISessionFeature>() is null);

        public string SessionId
        {
            get
            {
                var httpContext = _httpContextAccessor?.HttpContext;

                return IsSessionsAvailable
                    ? httpContext?.Session?.Id
                    : "0";
            }
        }

        public string GetSessionValue(string sessionName)
        {
            if(!IsSessionsAvailable) return null;
            return _httpContextAccessor.HttpContext?.Session.GetString(sessionName);
        }


        public void SetSessionValue(string sessionName, string value)
        {
            if(!IsSessionsAvailable) return;
            _httpContextAccessor.HttpContext?.Session.SetString(sessionName, value);
        }
    }
}
