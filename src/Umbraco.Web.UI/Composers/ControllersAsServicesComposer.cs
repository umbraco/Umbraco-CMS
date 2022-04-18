using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Web.UI.Composers
{
    /// <summary>
    /// Adds controllers to the service collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Umbraco 9 out of the box, makes use of <see cref="DefaultControllerActivator"/> which doesn't resolve controller
    /// instances from the IOC container, instead it resolves the required dependencies of the controller and constructs an instance
    /// of the controller.
    /// </para>
    /// <para>
    /// Some users may wish to switch to <see cref="ServiceBasedControllerActivator"/> (perhaps to make use of interception/decoration).
    /// </para>
    /// <para>
    /// This composer exists to help us detect ambiguous constructors in the CMS such that we do not cause unnecessary effort downstream.
    /// </para>
    /// <para>
    /// This Composer is not shipped by the Umbraco.Templates package.
    /// </para>
    /// </remarks>
    public class ControllersAsServicesComposer : IComposer
    {
        /// <inheritdoc />
        public void Compose(IUmbracoBuilder builder) => builder.Services
            .AddMvc()
            .AddControllersAsServicesWithoutChangingActivator();
    }

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
                builder.Services.TryAddTransient(controller, controller);
            }

            return builder;
        }
    }
}
