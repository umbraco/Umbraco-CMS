using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

public class FilePermissionsStep : StepBase, IInstallStep, IUpgradeStep
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

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private Task<Attempt<InstallationResult>> Execute()
    {
        // validate file permissions
        var permissionsOk =
            _filePermissionHelper.RunFilePermissionTestSuite(
                out Dictionary<FilePermissionTest, IEnumerable<string>> report);

        var translatedErrors =
            report.ToDictionary(x => _localizedTextService.Localize("permissions", x.Key), x => x.Value);
        if (permissionsOk == false)
        {
            IEnumerable<string> errorstring = translatedErrors.Select(x => $"{x.Key}: {string.Join(", ", x.Value)}");
            return Task.FromResult(FailWithMessage("Permission check failed:\n " + string.Join("\n", errorstring)));
        }

        return Task.FromResult(Success());
    }

    public Task<bool> RequiresExecutionAsync(InstallData model) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
