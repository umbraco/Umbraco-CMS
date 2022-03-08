using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Umbraco.Cms.Tests.Integration.Extensions
{
    internal static class MvcBuilderExtensions
    {
        /// <summary>
        /// <see cref="MvcCoreMvcBuilderExtensions.AddControllersAsServices"/> but without the replacement of
        /// <see cref="DefaultControllerActivator"/>.
        /// </summary>
        /// <remarks>
        /// We don't need to opt in to <see cref="ServiceBasedControllerActivator"/> to ensure container validation
        /// passes.
        /// </remarks>
        public static IMvcBuilder AddControllersAsServicesWithoutChangingActivator(this IMvcBuilder builder)
        {
            var feature = new ControllerFeature();
            builder.PartManager.PopulateFeature(feature);

            foreach (Type controller in feature.Controllers.Select(c => c.AsType()))
            {
                Console.WriteLine(controller.Name);
                builder.Services.TryAddTransient(controller, controller);
            }

            return builder;
        }
    }
}
