using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Common.Attributes;

namespace Umbraco.Web.Common.ApplicationModels
{
    // TODO: This should just exist in the back office project

    /// <summary>
    /// An application model provider for all Umbraco Back Office controllers
    /// </summary>
    public class BackOfficeApplicationModelProvider : IApplicationModelProvider
    {
        public BackOfficeApplicationModelProvider(IModelMetadataProvider modelMetadataProvider)
        {
            ActionModelConventions = new List<IActionModelConvention>()
            {
                new BackOfficeIdentityCultureConvention()
            };
        }

        /// <summary>
        /// Will execute after <see cref="DefaultApplicationModelProvider"/>
        /// </summary>
        public int Order => 0;

        public List<IActionModelConvention> ActionModelConventions { get; }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controller in context.Result.Controllers)
            {
                if (!IsBackOfficeController(controller))
                    continue;

                foreach (var action in controller.Actions)
                {
                    foreach (var convention in ActionModelConventions)
                    {
                        convention.Apply(action);
                    }
                }

            }
        }

        private bool IsBackOfficeController(ControllerModel controller)
            => controller.Attributes.OfType<IsBackOfficeAttribute>().Any();
        
    }
}
