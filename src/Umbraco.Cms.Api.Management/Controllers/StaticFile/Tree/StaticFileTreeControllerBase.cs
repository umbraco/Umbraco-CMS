using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

/// <summary>
/// Serves as the base controller for handling operations related to static file trees in the Umbraco CMS Management API.
/// Provides common functionality for derived controllers managing static files.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileTreeControllerBase : FileSystemTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticFileTreeControllerBase"/> class.
    /// </summary>
    /// <param name="fileSystemTreeService">The service used to read the static file tree, restricted to the allowed root folders.</param>
    public StaticFileTreeControllerBase(IPhysicalFileSystemTreeService fileSystemTreeService)
        : base(fileSystemTreeService)
    {
    }
}
