using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Search.Examine.Lucene;

/// <summary>
///     Sets the Examine <see cref="IApplicationRoot" /> to be ExamineIndexes sub directory of the Umbraco TEMP folder
/// </summary>
public class UmbracoApplicationRoot : IApplicationRoot
{
    private readonly IHostingEnvironment _hostingEnvironment;

    public UmbracoApplicationRoot(IHostingEnvironment hostingEnvironment)
        => _hostingEnvironment = hostingEnvironment;

    public DirectoryInfo ApplicationRoot
        => new(
            Path.Combine(
                _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData),
                "ExamineIndexes"));
}
