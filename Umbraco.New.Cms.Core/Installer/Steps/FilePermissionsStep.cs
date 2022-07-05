using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class FilePermissionsStep : IInstallStep
{
    private readonly IFilePermissionHelper _filePermissionHelper;
    private readonly ILocalizedTextService _localizedTextService;

    public FilePermissionsStep(
        IFilePermissionHelper filePermissionHelper,
        ILocalizedTextService localizedTextService)
    {
        _filePermissionHelper = filePermissionHelper;
        _localizedTextService = localizedTextService;
    }

    public InstallationType InstallationTypeTarget => InstallationType.NewInstall | InstallationType.Upgrade;

    public Task ExecuteAsync(InstallData model)
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

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
