using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.Common.ApplicationModels
{

    /// <summary>
    /// An application model provider for Umbraco API controllers to behave like WebApi controllers
    /// </summary>
    /// <remarks>
    /// <para>
    /// Conventions will be applied to controllers attributed with <see cref="UmbracoApiControllerAttribute"/>
    /// </para>
    /// <para>
    /// This is nearly a copy of aspnetcore's ApiBehaviorApplicationModelProvider which supplies a convention for the
    /// [ApiController] attribute, however that convention is too strict for our purposes so we will have our own.
    /// </para>
    /// <para>
    /// See https://shazwazza.com/post/custom-body-model-binding-per-controller-in-asp-net-core/
    /// and https://github.com/dotnet/aspnetcore/issues/21724
    /// </para>
    /// </remarks>
    public class UmbracoApiBehaviorApplicationModelProvider : IApplicationModelProvider
    {
        public UmbracoApiBehaviorApplicationModelProvider(IModelMetadataProvider modelMetadataProvider)
        {
            // see see https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1#apicontroller-attribute
            // for what these things actually do
            // NOTE: we don't have attribute routing requirements and we cannot use ApiVisibilityConvention without attribute routing

            ActionModelConventions = new List<IActionModelConvention>()
            {
                new ClientErrorResultFilterConvention(), // Ensures the responses without any body is converted into a simple json object with info instead of a string like "Status Code: 404; Not Found"
                new ConsumesConstraintForFormFileParameterConvention(), // If an controller accepts files, it must accept multipart/form-data.
                new InferParameterBindingInfoConvention(modelMetadataProvider), // no need for [FromBody] everywhere, A complex type parameter is assigned to FromBody

                // This ensures that all parameters of type BindingSource.Body (based on the above InferParameterBindingInfoConvention) are bound
                // using our own UmbracoJsonModelBinder
                new UmbracoJsonModelBinderConvention()
            };

            // TODO: Need to determine exactly how this affects errors
            var defaultErrorType = typeof(ProblemDetails);
            var defaultErrorTypeAttribute = new ProducesErrorResponseTypeAttribute(defaultErrorType);
            ActionModelConventions.Add(new ApiConventionApplicationModelConvention(defaultErrorTypeAttribute));
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
                if (!IsUmbracoApiController(controller))
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

        private bool IsUmbracoApiController(ControllerModel controller)
            => controller.Attributes.OfType<UmbracoApiControllerAttribute>().Any();
    }
}
