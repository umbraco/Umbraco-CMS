using System;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.Common.Controllers
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
