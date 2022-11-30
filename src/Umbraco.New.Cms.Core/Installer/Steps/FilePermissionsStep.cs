using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class FilePermissionsStep : IInstallStep, IUpgradeStep
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

    public Task ExecuteAsync(InstallData _) => Execute();

    public Task ExecuteAsync() => Execute();

    private Task Execute()
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

        return Task.CompletedTask;
    }

    public Task<bool> RequiresExecutionAsync(InstallData model) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
