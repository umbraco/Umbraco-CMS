using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Sets the Examine <see cref="IApplicationRoot" /> to be ExamineIndexes sub directory of the Umbraco TEMP folder
/// </summary>
public class UmbracoApplicationRoot : IApplicationRoot
{
    private readonly IHostEnvironment _hostEnvironment;

    // TODO (V20): Remove this obsolete constructor and the [ActivatorUtilitiesConstructor] attribute below.
    // Also revert the registration in UmbracoBuilderExtensions to:
    //   services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
    // (the factory form is required so [ActivatorUtilitiesConstructor] is honored).
    [Obsolete("Use the constructor accepting IHostEnvironment. Scheduled for removal in Umbraco 20.")]
    public UmbracoApplicationRoot(IHostingEnvironment hostingEnvironment)
        : this(StaticServiceProvider.Instance.GetRequiredService<IHostEnvironment>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public UmbracoApplicationRoot(IHostEnvironment hostEnvironment)
        => _hostEnvironment = hostEnvironment;

    public DirectoryInfo ApplicationRoot
        => new(Path.Combine(
            _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData),
            "ExamineIndexes"));
}
