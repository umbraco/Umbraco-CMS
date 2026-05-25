using Examine;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Sets the Examine <see cref="IApplicationRoot" /> to be ExamineIndexes sub directory of the Umbraco TEMP folder
/// </summary>
public class UmbracoApplicationRoot : IApplicationRoot
{
    private readonly IHostEnvironment _hostingEnvironment;

    public UmbracoApplicationRoot(IHostEnvironment hostingEnvironment)
        => _hostingEnvironment = hostingEnvironment;

    public DirectoryInfo ApplicationRoot
        => new(
            Path.Combine(
                _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData),
                "ExamineIndexes"));
}
