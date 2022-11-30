// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Install.InstallSteps;

/// <summary>
///     Represents a step in the installation that ensure all the required permissions on files and folders are correct.
/// </summary>
[Obsolete("Will be replace with a new step with the new backoffice")]
[InstallSetupStep(
    InstallationType.NewInstall | InstallationType.Upgrade,
    "Permissions",
    0,
    "",
    PerformsAppRestart = true)]
public class FilePermissionsStep : InstallSetupStep<object>
{
    private readonly IFilePermissionHelper _filePermissionHelper;
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilePermissionsStep" /> class.
    /// </summary>
    public FilePermissionsStep(
        IFilePermissionHelper filePermissionHelper,
        ILocalizedTextService localizedTextService)
    {
        _filePermissionHelper = filePermissionHelper;
        _localizedTextService = localizedTextService;
    }

    /// <inheritdoc />
    public override Task<InstallSetupResult?> ExecuteAsync(object model)
    {
        // validate file permissions
        var permissionsOk =
            _filePermissionHelper.RunFilePermissionTestSuite(
                out Dictionary<FilePermissionTest, IEnumerable<string>> report);

        var translatedErrors =
            report.ToDictionary(x => _localizedTextService.Localize("permissions", x.Key), x => x.Value);
        if (permissionsOk == false)
        {
            throw new InstallException("Permission check failed", "permissionsreport", new { errors = translatedErrors });
        }

        return Task.FromResult<InstallSetupResult?>(null);
    }

    /// <inheritdoc />
    public override bool RequiresExecution(object model) => true;
}
