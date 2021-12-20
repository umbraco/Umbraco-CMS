using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Services
{
    public class ConflictingRouteService : IConflictingRouteService
    {
        private readonly TypeLoader _typeLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictingRouteService"/> class.
        /// </summary>
        public ConflictingRouteService(TypeLoader typeLoader) => _typeLoader = typeLoader;

        /// <inheritdoc/>
        public bool HasConflictingRoutes()
        {
            var controllers = _typeLoader.GetTypes<UmbracoApiControllerBase>().ToList();
            // var pluginControllers = _typeLoader.GetTypes<PluginController>().ToList();
            // controllers.AddRange(pluginControllers);
            foreach (Type controller in controllers)
            {
                if (controllers.Count(x => x.Name == controller.Name) > 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
