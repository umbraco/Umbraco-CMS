using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Composing;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for Umbraco controllers.
    /// </summary>
    public abstract class UmbracoController : Controller
    {
        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

    }
}
