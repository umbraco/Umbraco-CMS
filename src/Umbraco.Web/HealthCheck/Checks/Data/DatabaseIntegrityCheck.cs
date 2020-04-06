using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Data
{
    [HealthCheck(
        "73DD0C1C-E0CA-4C31-9564-1DCA509788AF",
        "Database integrity check",
        Description = "Checks for various data integrity issues in the Umbraco database.",
        Group = "Data Integrity")]
    public class DatabaseIntegrityCheck : HealthCheck
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private const string _fixMediaPaths = "fixMediaPaths";
        private const string _fixContentPaths = "fixContentPaths";

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
                CheckContent(),
                CheckMedia()
            };
        }

        private HealthCheckStatus CheckMedia()
        {
            return CheckPaths(_fixMediaPaths, "Fix media paths", "media", () =>
            {
                var mediaPaths = _mediaService.VerifyNodePaths(out var invalidMediaPaths);
                return (mediaPaths, invalidMediaPaths);
            });
        }

        private HealthCheckStatus CheckContent()
        {
            return CheckPaths(_fixContentPaths, "Fix content paths", "content", () =>
            {
                var contentPaths = _contentService.VerifyNodePaths(out var invalidContentPaths);
                return (contentPaths, invalidContentPaths);
            });
        }

        private HealthCheckStatus CheckPaths(string actionAlias, string actionName, string entityType, Func<(bool success, int[] invalidPaths)> doCheck)
        {
            var result = doCheck();

            var actions = new List<HealthCheckAction>();
            if (!result.success)
            {
                actions.Add(new HealthCheckAction(actionAlias, Id)
                {
                    Name = actionName
                });
            }

            return new HealthCheckStatus(result.success
                ? $"All {entityType} paths are valid"
                : $"There are {result.invalidPaths.Length} invalid {entityType} paths")
            {
                ResultType = result.success ? StatusResultType.Success : StatusResultType.Error,
                Actions = actions
            };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case _fixContentPaths:
                    _contentService.FixNodePaths();
                    return CheckContent();
                case _fixMediaPaths:
                    _mediaService.FixNodePaths();
                    return CheckMedia();
                default:
                    throw new InvalidOperationException("Action not supported");
            }
        }
    }
}
