using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Sets the Examine <see cref="IApplicationRoot" /> to be ExamineIndexes sub directory of the Umbraco TEMP folder
/// </summary>
public class UmbracoApplicationRoot : IApplicationRoot
{
    private readonly IHostingEnvironment _hostingEnvironment;

    public UmbracoApplicationRoot(IHostingEnvironment hostingEnvironment)
        => _hostingEnvironment = hostingEnvironment;

    public DirectoryInfo ApplicationRoot
#pragma warning disable CS0618 // The IHostEnvironment.MapPathContentRoot extension changes which environment abstraction is used; preserve existing behaviour intentionally.
        => new(
            Path.Combine(
                _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData),
                "ExamineIndexes"));
#pragma warning restore CS0618
}
