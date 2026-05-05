namespace Umbraco.Cms.Infrastructure.IO;

/// <summary>
///     Helpers for splitting forward-slash separated file paths into name and parent components,
///     for use when calling the file-system services (<see cref="Umbraco.Cms.Core.Services.IPartialViewService"/>,
///     <see cref="Umbraco.Cms.Core.Services.IStylesheetService"/>, <see cref="Umbraco.Cms.Core.Services.IScriptService"/>).
/// </summary>
/// <remarks>
///     Used by package import (<see cref="Umbraco.Cms.Infrastructure.Packaging.PackageDataInstallation"/>) and the
///     partial-view populator (<see cref="Umbraco.Cms.Infrastructure.Templates.PartialViews.PartialViewPopulator"/>),
///     both of which receive paths that use '/' as a separator. <see cref="System.IO.Path.GetDirectoryName(string)"/>
///     is avoided because on Windows it normalises '/' to '\\' regardless of the input, producing a parent path with
///     a different separator from the source — fine for the underlying <c>PhysicalFileSystem</c> (which normalises
///     both separators) but inconsistent and surprising for callers reading the parent path.
/// </remarks>
internal static class ForwardSlashPath
{
    /// <summary>
    ///     Splits a forward-slash separated file path into a file name and a parent path.
    /// </summary>
    /// <param name="path">The path, expected to use '/' as a separator.</param>
    /// <returns>
    ///     A tuple containing the file name and the parent path, or <c>null</c> when the path has no parent segment.
    /// </returns>
    public static (string Name, string? ParentPath) Split(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return (path, null);
        }

        var parent = path[..lastSlash];
        return (path[(lastSlash + 1)..], string.IsNullOrEmpty(parent) ? null : parent);
    }
}
