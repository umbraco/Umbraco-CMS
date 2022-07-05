// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Plugins;

/// <summary>
///     Looks up files using the on-disk file system and check file extensions are on a allow list
/// </summary>
/// <remarks>
///     When the environment variable "DOTNET_USE_POLLING_FILE_WATCHER" is set to "1" or "true", calls to
///     <see cref="PhysicalFileProvider.Watch" /> will use <see cref="PollingFileChangeToken" />.
/// </remarks>
public class UmbracoPluginPhysicalFileProvider : PhysicalFileProvider, IFileProvider
{
    private UmbracoPluginSettings _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoPluginPhysicalFileProvider" /> class, at the given root
    ///     directory.
    /// </summary>
    /// <param name="root">The root directory. This should be an absolute path.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="filters">Specifies which files or directories are excluded.</param>
    public UmbracoPluginPhysicalFileProvider(string root, IOptionsMonitor<UmbracoPluginSettings> options, ExclusionFilters filters = ExclusionFilters.Sensitive)
        : base(root, filters)
    {
        _options = options.CurrentValue;
        options.OnChange(x =>
        {
            _options = x;
        });
    }

    /// <summary>
    ///     Locate a file at the given path by directly mapping path segments to physical directories.
    /// </summary>
    /// <remarks>
    ///     The path needs to pass the <see cref="ExclusionFilters" /> and the
    ///     <see cref="UmbracoPluginSettings.BrowsableFileExtensions" /> to be found.
    /// </remarks>
    /// <param name="subpath">A path under the root directory</param>
    /// <returns>The file information. Caller must check <see cref="IFileInfo.Exists" /> property. </returns>
    public new IFileInfo GetFileInfo(string subpath)
    {
        var extension = Path.GetExtension(subpath);
        var subPathInclAppPluginsFolder = Path.Combine(Core.Constants.SystemDirectories.AppPlugins, subpath);
        if (!_options.BrowsableFileExtensions.Contains(extension))
        {
            return new NotFoundFileInfo(subPathInclAppPluginsFolder);
        }

        return base.GetFileInfo(subPathInclAppPluginsFolder);
    }
}
