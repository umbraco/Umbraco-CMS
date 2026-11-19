// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

/// <summary>
/// Custom version of https://github.com/Shazwazza/Examine/blob/release/3.0/src/Examine.Lucene/Directories/TempEnvFileSystemDirectoryFactory.cs that includes the Umbraco SiteName property in the path hash
/// </summary>
public class UmbracoTempEnvFileSystemDirectoryFactory : FileSystemDirectoryFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoTempEnvFileSystemDirectoryFactory"/> class.
    /// </summary>
    /// <param name="applicationIdentifier">Identifies the running application instance, used to compute the temp-folder path.</param>
    /// <param name="lockFactory">The lock factory to use for the underlying file system directory.</param>
    /// <param name="hostingEnvironment">The hosting environment, used to read the site name for the temp-folder path.</param>
    /// <param name="indexOptions">The Lucene directory index options passed through to the base file system directory factory.</param>
    public UmbracoTempEnvFileSystemDirectoryFactory(
        IApplicationIdentifier applicationIdentifier,
        ILockFactory lockFactory,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        : base(new DirectoryInfo(GetTempPath(applicationIdentifier, hostingEnvironment)), lockFactory, indexOptions)
    {
    }

    /// <summary>
    /// Computes the temp-folder path index files are stored under, hashed from the site name and application identifier
    /// so a moved/relocated site does not pick up a stale index left behind by a previous worker.
    /// </summary>
    /// <param name="applicationIdentifier">Identifies the running application instance.</param>
    /// <param name="hostingEnvironment">The hosting environment, used to read the site name.</param>
    /// <returns>The temp-folder path for this application's Examine indexes.</returns>
    public static string GetTempPath(IApplicationIdentifier applicationIdentifier, IHostingEnvironment hostingEnvironment)
    {
        var hashString = hostingEnvironment.SiteName + "::" + applicationIdentifier.GetApplicationUniqueIdentifier();
        var appDomainHash = hashString.GenerateHash();

        var cachePath = Path.Combine(
            Path.GetTempPath(),
            "ExamineIndexes",
            //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
            // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
            // utilizing an old index
            appDomainHash);

        return cachePath;
    }
}
