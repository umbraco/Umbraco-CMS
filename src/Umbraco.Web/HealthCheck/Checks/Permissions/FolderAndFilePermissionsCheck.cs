using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Install;

namespace Umbraco.Web.HealthCheck.Checks.Permissions
{
    internal enum PermissionCheckRequirement
    {
        Required,
        Optional
    }

    internal enum PermissionCheckFor
    {
        Folder,
        File
    }

    [HealthCheck(
        "53DBA282-4A79-4B67-B958-B29EC40FCC23",
        "Folder & File Permissions",
        Description = "Checks that the web server folder and file permissions are set correctly for Umbraco to run.",
        Group = "Permissions")]
    public class FolderAndFilePermissionsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public FolderAndFilePermissionsCheck(ILocalizedTextService textService)
        {
            _textService = textService;
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
                { SystemDirectories.Data, PermissionCheckRequirement.Required },
                { SystemDirectories.Packages, PermissionCheckRequirement.Required},
                { SystemDirectories.Preview, PermissionCheckRequirement.Required },
                { SystemDirectories.AppPlugins, PermissionCheckRequirement.Required },
                { SystemDirectories.Config, PermissionCheckRequirement.Optional },
                { SystemDirectories.Css, PermissionCheckRequirement.Optional },
                { SystemDirectories.Media, PermissionCheckRequirement.Optional },
                { SystemDirectories.Scripts, PermissionCheckRequirement.Optional },
                { SystemDirectories.Umbraco, PermissionCheckRequirement.Optional },
                { SystemDirectories.MvcViews, PermissionCheckRequirement.Optional }
            };

            //These are special paths to check that will restart an app domain if a file is written to them,
            //so these need to be tested differently
            var pathsToCheckWithRestarts = new Dictionary<string, PermissionCheckRequirement>
            {
                { SystemDirectories.AppCode, PermissionCheckRequirement.Optional },
                { SystemDirectories.Bin, PermissionCheckRequirement.Optional }
            };

            // Run checks for required and optional paths for modify permission
            var requiredPathCheckResult = FilePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out var requiredFailedPaths);
            var optionalPathCheckResult = FilePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out var optionalFailedPaths);

            //now check the special folders
            var requiredPathCheckResult2 = FilePermissionHelper.EnsureDirectories(
                GetPathsToCheck(pathsToCheckWithRestarts, PermissionCheckRequirement.Required), out var requiredFailedPaths2, writeCausesRestart:true);
            var optionalPathCheckResult2 = FilePermissionHelper.EnsureDirectories(
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
            var requiredPathCheckResult = FilePermissionHelper.EnsureFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out requiredFailedPaths);
            var optionalPathCheckResult = FilePermissionHelper.EnsureFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out optionalFailedPaths);

            return GetStatus(requiredPathCheckResult, requiredFailedPaths, optionalPathCheckResult, optionalFailedPaths, PermissionCheckFor.File);
        }

        private static string[] GetPathsToCheck(Dictionary<string, PermissionCheckRequirement> pathsToCheck,
            PermissionCheckRequirement requirement)
        {
            return pathsToCheck
                .Where(x => x.Value == requirement)
                .Select(x => IOHelper.MapPath(x.Key))
                .OrderBy(x => x)
                .ToArray();
        }

        private HealthCheckStatus GetStatus(bool requiredPathCheckResult, IEnumerable<string> requiredFailedPaths,
            bool optionalPathCheckResult, IEnumerable<string> optionalFailedPaths,
            PermissionCheckFor checkingFor)
        {
            // Return error if any required paths fail the check, or warning if any optional ones do
            var resultType = StatusResultType.Success;
            var messageArea = "healthcheck";
            var messageAlias = string.Concat(checkingFor == PermissionCheckFor.Folder ? "folder" : "file", "PermissionsCheckMessage");
            var message = _textService.Localize(messageArea, messageAlias);
            if (requiredPathCheckResult == false)
            {
                resultType = StatusResultType.Error;
                messageAlias = string.Concat("required", checkingFor == PermissionCheckFor.Folder ? "Folder" : "File", "PermissionFailed");
                message = GetMessageForPathCheckFailure(messageArea, messageAlias, requiredFailedPaths);
            }
            else if (optionalPathCheckResult == false)
            {
                resultType = StatusResultType.Warning;
                messageAlias = string.Concat("optional", checkingFor == PermissionCheckFor.Folder ? "Folder" : "File", "PermissionFailed");
                message = GetMessageForPathCheckFailure(messageArea, messageAlias, optionalFailedPaths);
            }

            var actions = new List<HealthCheckAction>();
            return
                new HealthCheckStatus(message)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        private string GetMessageForPathCheckFailure(string messageArea,string messageAlias, IEnumerable<string> failedPaths)
        {
            var rootFolder = IOHelper.MapPath("/");
            var failedFolders = failedPaths
                .Select(x => ParseFolderFromFullPath(rootFolder, x));
            return _textService.Localize(messageArea, messageAlias,
                new[] { string.Join(", ", failedFolders) });
        }

        private string ParseFolderFromFullPath(string rootFolder, string filePath)
        {
            return filePath.Replace(rootFolder, string.Empty);
        }
    }
}
