using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.DataIntegrity
{
    /// <summary>
    /// U4-9544 Health check to detect if the database has any missing indexes or constraints
    /// </summary>
    [HealthCheck(
        "0873D589-2064-4EA3-A152-C43417FE00A4",
        "Database Schema Validation",
        Description = "This checks the Umbraco database by doing a comparison of current indexes and schema items with the current state of the database and returns any problems it found. Useful to detect if the database hasn't been upgraded correctly.",
        Group = "Data Integrity")]
    public class DatabaseSchemaValidationHealthCheck : HealthCheck
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ILocalizedTextService _textService;

        public DatabaseSchemaValidationHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _databaseContext = HealthCheckContext.ApplicationContext.DatabaseContext;
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return CheckDatabase();
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckDatabase() };
        }

        private HealthCheckStatus CheckDatabase()
        {
            var results = _databaseContext.ValidateDatabaseSchema();

            LogHelper.Warn(typeof(DatabaseSchemaValidationHealthCheck), _textService.Localize("databaseSchemaValidationCheckDatabaseLogMessage"));
            foreach(var error in results.Errors)
            {
                LogHelper.Warn(typeof(DatabaseSchemaValidationHealthCheck), error.Item1 + ": " + error.Item2);
            }

            if(results.Errors.Count > 0)
                return new HealthCheckStatus(_textService.Localize("healthcheck/databaseSchemaValidationCheckDatabaseErrors", new[] { results.Errors.Count.ToString() }))
                {
                    ResultType = StatusResultType.Error,
                    View = "Umbraco.Dashboard.DatabaseSchemaValidationController"
                };

            return new HealthCheckStatus(_textService.Localize("healthcheck/databaseSchemaValidationCheckDatabaseOk"))
            {
                ResultType = StatusResultType.Success
            };
        }
    }
}
