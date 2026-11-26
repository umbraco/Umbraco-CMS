// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

/// <summary>
///     Sets the Examine <see cref="IApplicationRoot" /> to be ExamineIndexes sub directory of the Umbraco TEMP folder
/// </summary>
public class UmbracoApplicationRoot : IApplicationRoot
{
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoApplicationRoot"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment used to resolve the TEMP folder path.</param>
    public UmbracoApplicationRoot(IHostingEnvironment hostingEnvironment)
        => _hostingEnvironment = hostingEnvironment;

    /// <inheritdoc />
    public DirectoryInfo ApplicationRoot
        => new(Path.Combine(
            _hostingEnvironment.MapPathContentRoot(Umbraco.Cms.Core.Constants.SystemDirectories.TempData),
            "ExamineIndexes"));
}
