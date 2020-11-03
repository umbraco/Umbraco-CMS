using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Mvc;

namespace Umbraco.Extensions
{
    public static class CompositionExtensions
    {
        public static void RegisterControllers(this Composition composition, IEnumerable<Type> controllerTypes)
        {
            foreach (var controllerType in controllerTypes)
                composition.Services.AddScoped(controllerType);
        }
    }
}
