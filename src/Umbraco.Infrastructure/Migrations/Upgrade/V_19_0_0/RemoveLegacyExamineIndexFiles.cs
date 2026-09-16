// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Best-effort removal of the on-disk Examine/Lucene index folder, whether or not the Examine search provider
/// is still installed.
/// </summary>
/// <remarks>
/// Examine rebuilds its indexes automatically on startup, so unconditionally clearing the folder is safe - the
/// alternative is that indexes under retired pre-search-abstraction names (e.g. <c>InternalIndex</c>,
/// <c>ExternalIndex</c>) are never read again by anything and pile up forever, since nothing else cleans them
/// up. Individual files or folders that cannot be deleted (e.g. because they're locked) are skipped rather than
/// failing the migration; anything left behind is safe to delete manually.
/// Folders whose name matches one of the current search abstraction index aliases
/// (<see cref="Core.Constants.IndexAliases"/>) are left untouched, since the Examine search provider still uses
/// those names for its active indexes.
/// </remarks>
public class RemoveLegacyExamineIndexFiles : UnscopedMigrationBase
{
    private static readonly HashSet<string> CurrentIndexNames = new(StringComparer.OrdinalIgnoreCase)
    {
        Core.Constants.IndexAliases.PublishedContent,
        Core.Constants.IndexAliases.DraftContent,
        Core.Constants.IndexAliases.DraftMedia,
        Core.Constants.IndexAliases.DraftMembers,
    };

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<RemoveLegacyExamineIndexFiles> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLegacyExamineIndexFiles"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <param name="logger">The typed logger.</param>
    public RemoveLegacyExamineIndexFiles(
        IMigrationContext context,
        IHostingEnvironment hostingEnvironment,
        ILogger<RemoveLegacyExamineIndexFiles> logger)
        : base(context)
    {
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override void Migrate()
    {
        var examineIndexesPath = Path.Combine(
            _hostingEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.TempData),
            "ExamineIndexes");

        RemoveDirectoryBestEffort(examineIndexesPath);

        Context.Complete();
    }

    private void RemoveDirectoryBestEffort(string path)
    {
        var root = new DirectoryInfo(path);
        if (root.Exists is false)
        {
            return;
        }

        var hadFailures = false;
        var hadCurrentIndexes = false;

        // Each index is a folder of files directly under the root - one level deep, no further nesting.
        foreach (DirectoryInfo indexFolder in root.GetDirectories())
        {
            if (CurrentIndexNames.Contains(indexFolder.Name))
            {
                hadCurrentIndexes = true;
                continue;
            }

            foreach (FileInfo file in indexFolder.GetFiles())
            {
                try
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }
                catch (Exception ex)
                {
                    hadFailures = true;
                    _logger.LogWarning(ex, "Could not remove legacy Examine index file {FileName}.", file.FullName);
                }
            }

            TryDeleteEmptyDirectory(indexFolder, ref hadFailures);
        }

        if (hadCurrentIndexes is false)
        {
            TryDeleteEmptyDirectory(root, ref hadFailures);
        }

        if (hadFailures)
        {
            _logger.LogWarning(
                "Could not fully remove the legacy Examine index folder at {Path}. Umbraco no longer reads these files - they can be safely deleted manually.",
                path);
        }
        else if (hadCurrentIndexes)
        {
            _logger.LogInformation("Removed legacy Examine index folders at {Path}, leaving active indexes in place.", path);
        }
        else
        {
            _logger.LogInformation("Removed legacy Examine index folder at {Path}.", path);
        }
    }

    private void TryDeleteEmptyDirectory(DirectoryInfo directory, ref bool hadFailures)
    {
        try
        {
            directory.Delete(recursive: false);
        }
        catch (Exception ex)
        {
            hadFailures = true;
            _logger.LogWarning(ex, "Could not remove legacy Examine index folder {FolderName}.", directory.FullName);
        }
    }
}
