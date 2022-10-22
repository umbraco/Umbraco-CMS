namespace Umbraco.Cms.Core.Hosting;

public interface IHostingEnvironment
{
    string SiteName { get; }

    /// <summary>
    ///     The unique application ID for this Umbraco website.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The returned value will be the same consistent value for an Umbraco website on a specific server and will the
    ///         same
    ///         between restarts of that Umbraco website/application on that specific server.
    ///     </para>
    ///     <para>
    ///         The value of this does not distinguish between unique workers/servers for this Umbraco application.
    ///         Usage of this must take into account that the same <see cref="ApplicationId" /> may be returned for the same
    ///         Umbraco website hosted on different servers.<br />
    ///         Similarly the usage of this must take into account that a different
    ///         <see cref="ApplicationId" /> may be returned for the same Umbraco website hosted on different servers.
    ///     </para>
    ///     <para>
    ///         This returns a hash of the value of IApplicationDiscriminator.Discriminator (which is most likely just the
    ///         value of <see cref=" Microsoft.Extensions.Hosting.IHostEnvironment.ContentRootPath" /> unless an alternative
    ///         implementation of IApplicationDiscriminator has been registered).<br />
    ///         However during ConfigureServices a temporary instance of IHostingEnvironment is constructed which guarantees
    ///         that this will be the hash of <see cref=" Microsoft.Extensions.Hosting.IHostEnvironment.ContentRootPath" />, so
    ///         the value may differ depend on when the property is used.
    ///     </para>
    ///     <para>
    ///         If you require this value during ConfigureServices it is probably a code smell.
    ///     </para>
    /// </remarks>
    [Obsolete("Please use IApplicationDiscriminator.Discriminator instead.")]
    string ApplicationId { get; }

    /// <summary>
    ///     Will return the physical path to the root of the application
    /// </summary>
    string ApplicationPhysicalPath { get; }

    string LocalTempPath { get; }

    /// <summary>
    ///     The web application's hosted path
    /// </summary>
    /// <remarks>
    ///     In most cases this will return "/" but if the site is hosted in a virtual directory then this will return the
    ///     virtual directory's path such as "/mysite".
    ///     This value must begin with a "/" and cannot end with "/".
    /// </remarks>
    string ApplicationVirtualPath { get; }

    bool IsDebugMode { get; }

    /// <summary>
    ///     Gets a value indicating whether Umbraco is hosted.
    /// </summary>
    bool IsHosted { get; }

    /// <summary>
    ///     Gets the main application url.
    /// </summary>
    Uri ApplicationMainUrl { get; }

    /// <summary>
    ///     Maps a virtual path to a physical path to the application's web root
    /// </summary>
    /// <remarks>
    ///     Depending on the runtime 'web root', this result can vary. For example in Net Framework the web root and the
    ///     content root are the same, however
    ///     in netcore the web root is /www therefore this will Map to a physical path within www.
    /// </remarks>
    [Obsolete("Please use the MapPathWebRoot extension method on an instance of IWebHostEnvironment instead")]
    string MapPathWebRoot(string path);

    /// <summary>
    ///     Maps a virtual path to a physical path to the application's root (not always equal to the web root)
    /// </summary>
    /// <remarks>
    ///     Depending on the runtime 'web root', this result can vary. For example in Net Framework the web root and the
    ///     content root are the same, however
    ///     in netcore the web root is /www therefore this will Map to a physical path within www.
    /// </remarks>
    [Obsolete(
        "Please use the MapPathContentRoot extension method on an instance of IHostEnvironment (or IWebHostEnvironment) instead")]
    string MapPathContentRoot(string path);

    /// <summary>
    ///     Converts a virtual path to an absolute URL path based on the application's web root
    /// </summary>
    /// <param name="virtualPath">The virtual path. Must start with either ~/ or / else an exception is thrown.</param>
    /// <remarks>
    ///     This maps the virtual path syntax to the web root. For example when hosting in a virtual directory called "site"
    ///     and the value "~/pages/test" is passed in, it will
    ///     map to "/site/pages/test" where "/site" is the value of <see cref="ApplicationVirtualPath" />.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     If virtualPath does not start with ~/ or /
    /// </exception>
    string ToAbsolute(string virtualPath);

    /// <summary>
    ///     Ensures that the application know its main Url.
    /// </summary>
    void EnsureApplicationMainUrl(Uri? currentApplicationUrl);
}
