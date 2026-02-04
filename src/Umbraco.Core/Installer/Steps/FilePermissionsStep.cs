using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

/// <summary>
/// An installation and upgrade step that validates file system permissions required by Umbraco.
/// </summary>
public class FilePermissionsStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IFilePermissionHelper _filePermissionHelper;
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePermissionsStep"/> class.
    /// </summary>
    /// <param name="filePermissionHelper">The file permission helper used to run permission tests.</param>
    /// <param name="localizedTextService">The localized text service for translating error messages.</param>
    public FilePermissionsStep(
        IFilePermissionHelper filePermissionHelper,
        ILocalizedTextService localizedTextService)
    {
        _filePermissionHelper = filePermissionHelper;
        _localizedTextService = localizedTextService;
    }

    /// <inheritdoc />
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    /// <inheritdoc />
    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    /// <summary>
    /// Executes the file permissions validation.
    /// </summary>
    /// <returns>A task containing an attempt with the installation result.</returns>
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

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync(InstallData model) => ShouldExecute();

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    /// <summary>
    /// Determines whether this step should be executed.
    /// </summary>
    /// <returns>A task containing <c>true</c> if the step should execute; otherwise, <c>false</c>.</returns>
    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
