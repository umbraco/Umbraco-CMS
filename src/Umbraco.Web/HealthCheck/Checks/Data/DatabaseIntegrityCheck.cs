using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Data
{
    [HealthCheck(
        "73DD0C1C-E0CA-4C31-9564-1DCA509788AF",
        "Database data integrity check",
        Description = "Checks for various data integrity issues in the Umbraco database.",
        Group = "Data Integrity")]
    public class DatabaseIntegrityCheck : HealthCheck
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private const string _fixMediaPaths = "fixMediaPaths";
        private const string _fixContentPaths = "fixContentPaths";
        private const string _fixMediaPathsTitle = "Fix media paths";
        private const string _fixContentPathsTitle = "Fix content paths";

        public DatabaseIntegrityCheck(IContentService contentService, IMediaService mediaService)
        {
            _contentService = contentService;
            _mediaService = mediaService;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[]
            {
                CheckDocuments(false),
                CheckMedia(false)
            };
        }

        private HealthCheckStatus CheckMedia(bool fix)
        {
            return CheckPaths(_fixMediaPaths, _fixMediaPathsTitle, Core.Constants.UdiEntityType.Media,
                () => _mediaService.CheckDataIntegrity(new ContentDataIntegrityReportOptions {FixIssues = fix}));
        }

        private HealthCheckStatus CheckDocuments(bool fix)
        {
            return CheckPaths(_fixContentPaths, _fixContentPathsTitle, Core.Constants.UdiEntityType.Document,
                () => _contentService.CheckDataIntegrity(new ContentDataIntegrityReportOptions {FixIssues = fix}));
        }

        private HealthCheckStatus CheckPaths(string actionAlias, string actionName, string entityType, Func<ContentDataIntegrityReport> doCheck)
        {
            var result = doCheck();

            var actions = new List<HealthCheckAction>();
            if (!result.Ok)
            {
                actions.Add(new HealthCheckAction(actionAlias, Id)
                {
                    Name = actionName
                });
            }

            return new HealthCheckStatus(result.Ok
                ? $"All {entityType} paths are valid"
                : $"There are {result.DetectedIssues.Count} invalid {entityType} paths")
            {
                ResultType = result.Ok ? StatusResultType.Success : StatusResultType.Error,
                Actions = actions
            };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return action.Alias switch
            {
                _fixContentPaths => CheckDocuments(true),
                _fixMediaPaths => CheckMedia(true),
                _ => throw new InvalidOperationException("Action not supported")
            };
        }
    }
}
