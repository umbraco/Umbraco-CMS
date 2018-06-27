using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "a3949eab-3932-4600-989e-24a6c5a127e4",
        "Developer files in website root",
        Description = "Checks to see if you have any common developers files in on your website which might leak sensitive data.",
        Group = "Security")]
    public class DeveloperFilesInRootCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public DeveloperFilesInRootCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
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
            return new[] { CheckForFiles() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "removeDeveloperFilesFromRoot":
                    return RemoveFiles();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus RemoveFiles()
        {
            var success = false;
            var message = string.Empty;

            try
            {
                var foundFiles = FindFiles();
                if (foundFiles.Any()) {
                    foreach ( var file in foundFiles )
                    {
                        File.Delete(HostingEnvironment.MapPath(file));
                    }
                }
                success = true;
                message = _textService.Localize("healthcheck/developerFilesRemovedFromRoot");
            }
            catch (Exception exception)
            {
                LogHelper.Error<DeveloperFilesInRootCheck>("Could not delete developer files from root of site", exception);
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = new List<HealthCheckAction>()
                };
        }

        private List<string> FindFiles()
        {
            var filesToCheck = new string[]
            {
                "~/package.json",
                "~/package-lock.json",
                "~/gruntfile.js",
                "~/manifest.json",
                "~/packages.config",
                "~/webpack.config.js",
                "~/readme.txt",
                "~/readme.doc",
                "~/readme.md",
                "~/read-me.txt",
                "~/read-me.doc",
                "~/read-me.md",
                "~/read_me.txt",
                "~/read_me.doc",
                "~/read_me.md"
            };

            var foundFiles = new List<string>();

            foreach (var file in filesToCheck)
            {
                if (File.Exists(HostingEnvironment.MapPath(file)))
                {
                    foundFiles.Add(file);
                }
            }

            return foundFiles;
        }

        private HealthCheckStatus CheckForFiles()
        {
            var message = string.Empty;
            var foundFiles = FindFiles();

            if (foundFiles.Any())
            {
                message = _textService.Localize("healthcheck/developerFilesInRootFound", new[] { String.Join("</li><li>", foundFiles.ToArray<string>()) });
            }
            else
            {
                message = _textService.Localize("healthcheck/developerFilesInRootNotFound");
            }

            var actions = new List<HealthCheckAction>();
            actions.Add(new HealthCheckAction("removeDeveloperFilesFromRoot", Id)
            // Override the "Rectify" button name and describe what this action will do
            {
                Name = _textService.Localize("healthcheck/developerFilesInRootActionButton"),
                Description = _textService.Localize("healthcheck/developerFilesInRootActionDescription")
            });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = !foundFiles.Any() ? StatusResultType.Success : StatusResultType.Warning,
                    Actions = actions
                };
        }
    }
}


