using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Install;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks.Permissions
{
    [HealthCheck(
        "53DBA282-4A79-4B67-B958-B29EC40FCC23",
        "Folder & File Permissions",
        Description = "Checks that the web server folder and file permissions are set correctly for Umbraco to run.",
        Group = "Permissions")]
    public class FolderAndFilePermissionsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IFilePermissionHelper _filePermissionHelper;
        private readonly IIOHelper _ioHelper;

        public FolderAndFilePermissionsCheck(ILocalizedTextService textService, IOptionsMonitor<GlobalSettings> globalSettings, IFilePermissionHelper filePermissionHelper, IIOHelper ioHelper)
        {
            _textService = textService;
            _globalSettings = globalSettings;
            _filePermissionHelper = filePermissionHelper;
            _ioHelper = ioHelper;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckFolderPermissions(), CheckFilePermissions() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("FolderAndFilePermissionsCheck has no executable actions");
        }

        private HealthCheckStatus CheckFolderPermissions()
        {
            // Create lists of paths to check along with a flag indicating if modify rights are required
            // in ALL circumstances or just some
            var pathsToCheck = new Dictionary<string, PermissionCheckRequirement>
            {
                { Constants.SystemDirectories.Data, PermissionCheckRequirement.Required },
                { Constants.SystemDirectories.Packages, PermissionCheckRequirement.Required},
                { Constants.SystemDirectories.Preview, PermissionCheckRequirement.Required },
                { Constants.SystemDirectories.AppPlugins, PermissionCheckRequirement.Required },
                { Constants.SystemDirectories.Config, PermissionCheckRequirement.Optional },
                { _globalSettings.CurrentValue.UmbracoCssPath, PermissionCheckRequirement.Optional },
                { _globalSettings.CurrentValue.UmbracoMediaPath, PermissionCheckRequirement.Optional },
                { _globalSettings.CurrentValue.UmbracoScriptsPath, PermissionCheckRequirement.Optional },
                { _globalSettings.CurrentValue.UmbracoPath, PermissionCheckRequirement.Optional },
                { Constants.SystemDirectories.MvcViews, PermissionCheckRequirement.Optional }
            };

            //These are special paths to check that will restart an app domain if a file is written to them,
            //so these need to be tested differently
            var pathsToCheckWithRestarts = new Dictionary<string, PermissionCheckRequirement>
            {
                { Constants.SystemDirectories.AppCode, PermissionCheckRequirement.Optional },
                { Constants.SystemDirectories.Bin, PermissionCheckRequirement.Optional }
            };

            // Run checks for required and optional paths for modify permission
            var requiredPathCheckResult = _filePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out var requiredFailedPaths);
            var optionalPathCheckResult = _filePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out var optionalFailedPaths);

            //now check the special folders
            var requiredPathCheckResult2 = _filePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheckWithRestarts, PermissionCheckRequirement.Required), out var requiredFailedPaths2, writeCausesRestart: true);
            var optionalPathCheckResult2 = _filePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheckWithRestarts, PermissionCheckRequirement.Optional), out var optionalFailedPaths2, writeCausesRestart: true);

            requiredPathCheckResult = requiredPathCheckResult && requiredPathCheckResult2;
            optionalPathCheckResult = optionalPathCheckResult && optionalPathCheckResult2;

            //combine the paths
            requiredFailedPaths = requiredFailedPaths.Concat(requiredFailedPaths2).ToList();
            optionalFailedPaths = requiredFailedPaths.Concat(optionalFailedPaths2).ToList();

            return GetStatus(requiredPathCheckResult, requiredFailedPaths, optionalPathCheckResult, optionalFailedPaths, PermissionCheckFor.Folder);
        }

        private HealthCheckStatus CheckFilePermissions()
        {
            // Create lists of paths to check along with a flag indicating if modify rights are required
            // in ALL circumstances or just some
            var pathsToCheck = new Dictionary<string, PermissionCheckRequirement>
            {
                { "~/Web.config", PermissionCheckRequirement.Optional },
            };

            // Run checks for required and optional paths for modify permission
            IEnumerable<string> requiredFailedPaths;
            IEnumerable<string> optionalFailedPaths;
            var requiredPathCheckResult = _filePermissionHelper.EnsureFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out requiredFailedPaths);
            var optionalPathCheckResult = _filePermissionHelper.EnsureFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out optionalFailedPaths);

            return GetStatus(requiredPathCheckResult, requiredFailedPaths, optionalPathCheckResult, optionalFailedPaths, PermissionCheckFor.File);
        }

        private string[] GetPathsToCheck(Dictionary<string, PermissionCheckRequirement> pathsToCheck,
            PermissionCheckRequirement requirement)
        {
            return pathsToCheck
                .Where(x => x.Value == requirement)
                .Select(x => _ioHelper.MapPath(x.Key))
                .OrderBy(x => x)
                .ToArray();
        }

        private HealthCheckStatus GetStatus(bool requiredPathCheckResult, IEnumerable<string> requiredFailedPaths, bool optionalPathCheckResult, IEnumerable<string> optionalFailedPaths, PermissionCheckFor checkingFor)
        {
            // Return error if any required paths fail the check, or warning if any optional ones do
            var resultType = StatusResultType.Success;
            var messageKey = string.Format("healthcheck/{0}PermissionsCheckMessage",
                checkingFor == PermissionCheckFor.Folder ? "folder" : "file");
            var message = _textService.Localize(messageKey);
            if (requiredPathCheckResult == false)
            {
                resultType = StatusResultType.Error;
                messageKey = string.Format("healthcheck/required{0}PermissionFailed",
                    checkingFor == PermissionCheckFor.Folder ? "Folder" : "File");
                message = GetMessageForPathCheckFailure(messageKey, requiredFailedPaths);
            }
            else if (optionalPathCheckResult == false)
            {
                resultType = StatusResultType.Warning;
                messageKey = string.Format("healthcheck/optional{0}PermissionFailed",
                    checkingFor == PermissionCheckFor.Folder ? "Folder" : "File");
                message = GetMessageForPathCheckFailure(messageKey, optionalFailedPaths);
            }

            var actions = new List<HealthCheckAction>();
            return new HealthCheckStatus(message)
            {
                ResultType = resultType,
                Actions = actions
            };
        }

        private string GetMessageForPathCheckFailure(string messageKey, IEnumerable<string> failedPaths)
        {
            var rootFolder = _ioHelper.MapPath("/");
            var failedFolders = failedPaths
                .Select(x => ParseFolderFromFullPath(rootFolder, x));
            return _textService.Localize(messageKey,
                new[] { string.Join(", ", failedFolders) });
        }

        private string ParseFolderFromFullPath(string rootFolder, string filePath)
        {
            return filePath.Replace(rootFolder, string.Empty);
        }
    }
}
