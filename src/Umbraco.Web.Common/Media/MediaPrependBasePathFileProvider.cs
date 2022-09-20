using Dazinator.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Umbraco.Cms.Web.Common.Media;

/// <summary>
/// Prepends a base path to files / directories from an underlying file provider.
/// </summary>
/// <remarks>
/// This is a clone-and-own of PrependBasePathFileProvider from the Dazinator project, cleaned up and tweaked to work
/// for serving media files with special characters.
/// Reference issue: https://github.com/umbraco/Umbraco-CMS/issues/12903
/// A PR has been submitted to the Dazinator project: https://github.com/dazinator/Dazinator.Extensions.FileProviders/pull/53
/// If that PR is accepted, the Dazinator dependency should be updated and this class should be removed.
/// </remarks>
internal class MediaPrependBasePathFileProvider : IFileProvider
{
    private readonly PathString _basePath;
    private readonly IFileProvider _underlyingFileProvider;
    private readonly IFileInfo _baseDirectoryFileInfo;
    private static readonly char[] _splitChar = { '/' };

    public MediaPrependBasePathFileProvider(string? basePath, IFileProvider underlyingFileProvider)
    {
        _basePath = new PathString(basePath);
        _baseDirectoryFileInfo = new DirectoryFileInfo(_basePath.ToString().TrimStart(_splitChar));
        _underlyingFileProvider = underlyingFileProvider;
    }

    protected virtual bool TryMapSubPath(string originalSubPath, out PathString newSubPath)
    {
        if (!string.IsNullOrEmpty(originalSubPath))
        {
            PathString originalPathString;
            originalPathString = originalSubPath[0] != '/' ? new PathString('/' + originalSubPath) : new PathString(originalSubPath);

            if (originalPathString.HasValue && originalPathString.StartsWithSegments(_basePath, out PathString remaining))
            {
                // var childPath = originalPathString.Remove(0, _basePath.Value.Length);
                newSubPath = remaining;
                return true;
            }
        }

        newSubPath = null;
        return false;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (string.IsNullOrEmpty(subpath))
        {
            // return root / base directory.
            return new EnumerableDirectoryContents(_baseDirectoryFileInfo);
        }

        if (TryMapSubPath(subpath, out PathString newPath))
        {
            IDirectoryContents? contents = _underlyingFileProvider.GetDirectoryContents(newPath);
            return contents;
        }

        return new NotFoundDirectoryContents();
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (TryMapSubPath(subpath, out PathString newPath))
        {
            // KJA changed: use explicit newPath.Value instead of implicit newPath string operator (which calls ToString())
            IFileInfo? result = _underlyingFileProvider.GetFileInfo(newPath.Value);
            return result;
        }

        return new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        // We check if the pattern starts with the base path, and remove it if necessary.
        // otherwise we just pass the pattern through unaltered.
        if (TryMapSubPath(filter, out PathString newPath))
        {
            // KJA changed: use explicit newPath.Value instead of implicit newPath string operator (which calls ToString())
            IChangeToken? result = _underlyingFileProvider.Watch(newPath.Value);
            return result;
        }

        return _underlyingFileProvider.Watch(newPath);
    }
}

