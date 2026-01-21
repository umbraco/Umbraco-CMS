using System.Text;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides utilities for manipulating URIs in the Umbraco routing context.
/// </summary>
public sealed class UriUtility
{
    private static string? _appPath;
    private static string? _appPathPrefix;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UriUtility" /> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    public UriUtility(IHostingEnvironment hostingEnvironment)
    {
        if (hostingEnvironment is null)
        {
            throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        ResetAppDomainAppVirtualPath(hostingEnvironment);
    }

    /// <summary>
    ///     Gets the application path. Will be "/" or "/foo".
    /// </summary>
    public string? AppPath => _appPath;

    /// <summary>
    ///     Gets the application path prefix. Will be "" or "/foo".
    /// </summary>
    public string? AppPathPrefix => _appPathPrefix;

    /// <summary>
    ///     Converts a relative URL to an absolute URL by prepending the application path prefix.
    /// </summary>
    /// <param name="url">The relative URL.</param>
    /// <returns>The absolute URL with the application path prefix.</returns>
    public string ToAbsolute(string url)
    {
        // return ResolveUrl(url);
        url = url.TrimStart(Constants.CharArrays.Tilde);
        return _appPathPrefix + url;
    }

    /// <summary>
    ///     Sets the application virtual path. Internal for unit testing only.
    /// </summary>
    /// <param name="appPath">The application path to set.</param>
    internal void SetAppDomainAppVirtualPath(string appPath)
    {
        _appPath = appPath ?? "/";
        _appPathPrefix = _appPath;
        if (_appPathPrefix == "/")
        {
            _appPathPrefix = string.Empty;
        }
    }

    /// <summary>
    ///     Resets the application virtual path from the hosting environment.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    internal void ResetAppDomainAppVirtualPath(IHostingEnvironment hostingEnvironment) =>
        SetAppDomainAppVirtualPath(hostingEnvironment.ApplicationVirtualPath);

    /// <summary>
    ///     Converts a virtual path to an application-relative path by stripping the virtual directory if present.
    /// </summary>
    /// <param name="virtualPath">The virtual path.</param>
    /// <returns>The application-relative path.</returns>
    public string ToAppRelative(string virtualPath)
    {
        if (_appPathPrefix is not null && virtualPath.InvariantStartsWith(_appPathPrefix)
                                       && (virtualPath.Length == _appPathPrefix.Length ||
                                           virtualPath[_appPathPrefix.Length] == '/'))
        {
            virtualPath = virtualPath[_appPathPrefix.Length..];
        }

        if (virtualPath.Length == 0)
        {
            virtualPath = "/";
        }

        return virtualPath;
    }

    /// <summary>
    ///     Maps an internal Umbraco URI to a public URI with virtual directory and appropriate suffixes.
    /// </summary>
    /// <param name="uri">The internal Umbraco URI.</param>
    /// <param name="requestConfig">The request handler settings.</param>
    /// <returns>The public URI.</returns>
    public Uri UriFromUmbraco(Uri uri, RequestHandlerSettings requestConfig)
    {
        var path = uri.GetSafeAbsolutePath();

        if (path != "/" && requestConfig.AddTrailingSlash)
        {
            path = path.EnsureEndsWith("/");
        }

        path = ToAbsolute(path);

        return uri.Rewrite(path);
    }

    /// <summary>
    ///     Maps a media Umbraco URI to a public URI with virtual directory.
    /// </summary>
    /// <param name="uri">The media URI.</param>
    /// <returns>The public media URI.</returns>
    public Uri MediaUriFromUmbraco(Uri uri)
    {
        var path = uri.GetSafeAbsolutePath();
        path = ToAbsolute(path);
        return uri.Rewrite(path);
    }

    /// <summary>
    ///     Maps a public URI to an internal Umbraco URI without virtual directory, lowercased.
    /// </summary>
    /// <param name="uri">The public URI.</param>
    /// <returns>The internal Umbraco URI.</returns>
    public Uri UriToUmbraco(Uri uri)
    {
        // TODO: This is critical code that executes on every request, we should
        // look into if all of this is necessary? not really sure we need ToLower?

        // note: no need to decode uri here because we're returning a uri
        // so it will be re-encoded anyway
        var path = uri.GetSafeAbsolutePath();

        path = path.ToLower();
        path = ToAppRelative(path); // strip vdir if any

        if (path != "/")
        {
            path = path.TrimEnd(Constants.CharArrays.ForwardSlash);

            // perform fallback to root if the path was all slashes (i.e. https://some.where//////)
            if (path == string.Empty)
            {
                path = "/";
            }
        }

        return uri.Rewrite(path);
    }

    #region ResolveUrl

    /// <summary>
    ///     Resolves a relative URL to an absolute URL.
    /// </summary>
    /// <param name="relativeUrl">The relative URL to resolve.</param>
    /// <returns>The resolved URL.</returns>
    /// <remarks>
    ///     If browsing http://example.com/sub/page1.aspx then
    ///     ResolveUrl("page2.aspx") returns "/page2.aspx".
    /// </remarks>
    public string ResolveUrl(string relativeUrl)
    {
        if (relativeUrl == null)
        {
            throw new ArgumentNullException("relativeUrl");
        }

        if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
        {
            return relativeUrl;
        }

        var idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
        if (idxOfScheme != -1)
        {
            var idxOfQM = relativeUrl.IndexOf('?', StringComparison.Ordinal);
            if (idxOfQM == -1 || idxOfQM > idxOfScheme)
            {
                return relativeUrl;
            }
        }

        var sbUrl = new StringBuilder();
        sbUrl.Append(_appPathPrefix);
        if (sbUrl.Length == 0 || sbUrl[^1] != '/')
        {
            sbUrl.Append('/');
        }

        // found question mark already? query string, do not touch!
        var foundQM = false;
        bool foundSlash; // the latest char was a slash?
        if (relativeUrl.Length > 1
            && relativeUrl[0] == '~'
            && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
        {
            relativeUrl = relativeUrl[2..];
            foundSlash = true;
        }
        else
        {
            foundSlash = false;
        }

        foreach (var c in relativeUrl)
        {
            if (!foundQM)
            {
                if (c == '?')
                {
                    foundQM = true;
                }
                else
                {
                    if (c == '/' || c == '\\')
                    {
                        if (foundSlash)
                        {
                            continue;
                        }

                        sbUrl.Append('/');
                        foundSlash = true;
                        continue;
                    }

                    if (foundSlash)
                    {
                        foundSlash = false;
                    }
                }
            }

            sbUrl.Append(c);
        }

        return sbUrl.ToString();
    }

    #endregion

    /// <summary>
    ///     Returns an full URL with the host, port, etc...
    /// </summary>
    /// <param name="absolutePath">An absolute path (i.e. starts with a '/' )</param>
    /// <param name="curentRequestUrl"> </param>
    /// <returns></returns>
    /// <remarks>
    ///     Based on http://stackoverflow.com/questions/3681052/get-absolute-url-from-relative-path-refactored-method
    /// </remarks>
    internal Uri ToFullUrl(string absolutePath, Uri curentRequestUrl)
    {
        if (string.IsNullOrEmpty(absolutePath))
        {
            throw new ArgumentNullException(nameof(absolutePath));
        }

        if (!absolutePath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new FormatException("The absolutePath specified does not start with a '/'");
        }

        return new Uri(absolutePath, UriKind.Relative).MakeAbsolute(curentRequestUrl);
    }
}
