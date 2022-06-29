using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Install.NewInstallSteps;

public class NewFilePermissionsStep : NewInstallSetupStep
{
    private readonly IFilePermissionHelper _filePermissionHelper;
    private readonly ILocalizedTextService _localizedTextService;

    public NewFilePermissionsStep(
        IFilePermissionHelper filePermissionHelper,
        ILocalizedTextService localizedTextService)
        : base(
            "Permissions",
            10,
            InstallationType.NewInstall | InstallationType.Upgrade)
    {
        _filePermissionHelper = filePermissionHelper;
        _localizedTextService = localizedTextService;
    }

    public override Task ExecuteAsync(InstallData model)
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

    public override Task<bool> RequiresExecution(InstallData model) => throw new NotImplementedException();
}
