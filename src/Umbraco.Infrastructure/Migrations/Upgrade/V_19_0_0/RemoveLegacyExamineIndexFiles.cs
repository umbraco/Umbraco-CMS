// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Best-effort removal of the on-disk Examine/Lucene index folders for the indexes that were retired with the
/// introduction of the search abstraction, whether or not the Examine search provider is still installed.
/// </summary>
/// <remarks>
/// Indexes under the retired names (<c>InternalIndex</c>, <c>ExternalIndex</c>, <c>MembersIndex</c> and
/// <c>DeliveryApiContentIndex</c>) are never read again by anything and would otherwise pile up forever.
/// Any other index folder - current search abstraction indexes as well as custom indexes - is left untouched,
/// as it may still be in use. The root index folder is only removed if it ends up empty.
/// Individual files or folders that cannot be deleted (e.g. because they're locked) are skipped rather than
/// failing the migration; anything left behind is safe to delete manually.
/// </remarks>
public class RemoveLegacyExamineIndexFiles : UnscopedMigrationBase
{
    private static readonly HashSet<string> LegacyIndexNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "InternalIndex",
        "ExternalIndex",
        "MembersIndex",
        "DeliveryApiContentIndex",
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
        var hasOtherIndexes = false;

        // Each index is a folder of files directly under the root - one level deep, no further nesting.
        foreach (DirectoryInfo indexFolder in root.GetDirectories())
        {
            if (LegacyIndexNames.Contains(indexFolder.Name) is false)
            {
                hasOtherIndexes = true;
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

        if (hasOtherIndexes is false)
        {
            TryDeleteEmptyDirectory(root, ref hadFailures);
        }

        if (hadFailures)
        {
            _logger.LogWarning(
                "Could not fully remove the legacy Examine index folders at {Path}. Umbraco no longer reads these files - they can be safely deleted manually.",
                path);
        }
        else if (hasOtherIndexes)
        {
            _logger.LogInformation("Removed legacy Examine index folders at {Path}, leaving other index folders in place.", path);
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
