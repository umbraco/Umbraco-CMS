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

        public FolderAndFilePermissionsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
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
                { SystemDirectories.AppCode, PermissionCheckRequirement.Optional },
                { SystemDirectories.Data, PermissionCheckRequirement.Required },
                { SystemDirectories.Packages, PermissionCheckRequirement.Required},
                { SystemDirectories.Preview, PermissionCheckRequirement.Required },
                { SystemDirectories.AppPlugins, PermissionCheckRequirement.Required },
                { SystemDirectories.Bin, PermissionCheckRequirement.Optional },
                { SystemDirectories.Config, PermissionCheckRequirement.Optional },
                { SystemDirectories.Css, PermissionCheckRequirement.Optional },
                { SystemDirectories.Masterpages, PermissionCheckRequirement.Optional },
                { SystemDirectories.Media, PermissionCheckRequirement.Optional },
                { SystemDirectories.Scripts, PermissionCheckRequirement.Optional },
                { SystemDirectories.Umbraco, PermissionCheckRequirement.Optional },
                { SystemDirectories.UmbracoClient, PermissionCheckRequirement.Optional },
                { SystemDirectories.UserControls, PermissionCheckRequirement.Optional },
                { SystemDirectories.MvcViews, PermissionCheckRequirement.Optional },
                { SystemDirectories.Xslt, PermissionCheckRequirement.Optional },
            };

            // Run checks for required and optional paths for modify permission
            List<string> requiredFailedPaths;
            List<string> optionalFailedPaths;
            var requiredPathCheckResult = FilePermissionHelper.TestDirectories(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out requiredFailedPaths);
            var optionalPathCheckResult = FilePermissionHelper.TestDirectories(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out optionalFailedPaths);

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
            List<string> requiredFailedPaths;
            List<string> optionalFailedPaths;
            var requiredPathCheckResult = FilePermissionHelper.TestFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Required), out requiredFailedPaths);
            var optionalPathCheckResult = FilePermissionHelper.TestFiles(GetPathsToCheck(pathsToCheck, PermissionCheckRequirement.Optional), out optionalFailedPaths);

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

        private HealthCheckStatus GetStatus(bool requiredPathCheckResult, List<string> requiredFailedPaths,
            bool optionalPathCheckResult, IEnumerable<string> optionalFailedPaths,
            PermissionCheckFor checkingFor)
        {
            // Return error if any required parths fail the check, or warning if any optional ones do
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
            return
                new HealthCheckStatus(message)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        private string GetMessageForPathCheckFailure(string messageKey, IEnumerable<string> failedPaths)
        {
            var rootFolder = IOHelper.MapPath("/");
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