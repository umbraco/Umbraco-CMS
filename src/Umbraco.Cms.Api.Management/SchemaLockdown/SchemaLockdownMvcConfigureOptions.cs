using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Registers <see cref="SchemaLockdownConvention"/> with MVC.
/// </summary>
/// <remarks>
/// <see cref="MvcOptions.Conventions"/> only accepts <see cref="IApplicationModelConvention"/>. Wrapping
/// <see cref="SchemaLockdownConvention"/> (an <see cref="IControllerModelConvention"/>) in an
/// <see cref="IApplicationModelConvention"/> we own, rather than via the framework's own wrapping extension, lets the
/// double-registration guard below find the wrapper again by type on the same <see cref="MvcOptions"/> instance.
/// </remarks>
internal class SchemaLockdownMvcConfigureOptions : IConfigureOptions<MvcOptions>
{
    private readonly SchemaLockdownConvention _convention;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownMvcConfigureOptions"/> class.
    /// </summary>
    /// <param name="convention">The convention to register.</param>
    public SchemaLockdownMvcConfigureOptions(SchemaLockdownConvention convention) => _convention = convention;

    /// <inheritdoc />
    public void Configure(MvcOptions options)
    {
        if (options.Conventions.Any(convention => convention is ControllerModelConventionAdapter))
        {
            return;
        }

        options.Conventions.Add(new ControllerModelConventionAdapter(_convention));
    }

    private sealed class ControllerModelConventionAdapter : IApplicationModelConvention
    {
        private readonly IControllerModelConvention _convention;

        public ControllerModelConventionAdapter(IControllerModelConvention convention) => _convention = convention;

        public void Apply(ApplicationModel application)
        {
            foreach (ControllerModel controller in application.Controllers)
            {
                _convention.Apply(controller);
            }
        }
    }
}
