namespace Umbraco.Cms.Infrastructure.IO;

/// <summary>
///     Helpers for splitting file paths into name and parent components,
///     for use when calling the file-system services (<see cref="Umbraco.Cms.Core.Services.IPartialViewService"/>,
///     <see cref="Umbraco.Cms.Core.Services.IStylesheetService"/>, <see cref="Umbraco.Cms.Core.Services.IScriptService"/>).
/// </summary>
/// <remarks>
///     Used by package import (<see cref="Umbraco.Cms.Infrastructure.Packaging.PackageDataInstallation"/>) and the
///     partial-view populator (<see cref="Umbraco.Cms.Infrastructure.Templates.PartialViews.PartialViewPopulator"/>).
///     Inputs may use '/' or '\' as a separator (the Umbraco backoffice serialises stylesheet/script/partial-view
///     paths into <c>package.xml</c> using the host platform's separator, so packages built on Windows contain '\').
///     <see cref="System.IO.Path.GetDirectoryName(string)"/> is avoided because on Windows it normalises '/' to '\\'
///     regardless of the input — fine for the underlying <c>PhysicalFileSystem</c> (which normalises both separators)
///     but inconsistent and surprising for callers reading the parent path. Returned parent paths are always
///     '/'-normalised to match the rest of the Umbraco file-system convention.
/// </remarks>
internal static class FileSystemPath
{
    /// <summary>
    ///     Splits a file path into a file name and a parent path.
    /// </summary>
    /// <param name="path">The path. May use '/' or '\' as a separator.</param>
    /// <returns>
    ///     A tuple containing the file name and the parent path (always '/'-normalised), or <c>null</c> when the path
    ///     has no parent segment.
    /// </returns>
    public static (string Name, string? ParentPath) Split(string path)
    {
        var lastSeparator = path.LastIndexOfAny(['/', '\\']);
        if (lastSeparator < 0)
        {
            return (path, null);
        }

        var parent = path[..lastSeparator].Replace('\\', '/');
        return (path[(lastSeparator + 1)..], string.IsNullOrEmpty(parent) ? null : parent);
    }
}
