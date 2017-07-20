using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Install.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.Upgrade, "MajorVersion7UpgradeReport", 1, "")]
    internal class MajorVersion7UpgradeReport : InstallSetupStep<object>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IDatabaseContext _databaseContext;
        private readonly IScopeProvider _scopeProvider;
        private readonly IRuntimeState _runtime;

        public MajorVersion7UpgradeReport(DatabaseBuilder databaseBuilder, IRuntimeState runtime, IDatabaseContext databaseContext, IScopeProvider scopeProvider)
        {
            _databaseBuilder = databaseBuilder;
            _runtime = runtime;
            _databaseContext = databaseContext;
            _scopeProvider = scopeProvider;
        }

        public override InstallSetupResult Execute(object model)
        {
            //we cannot run this step if the db is not configured.
            if (_databaseBuilder.IsDatabaseConfigured == false)
            {
                return null;
            }

            var result = _databaseBuilder.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();

            return new InstallSetupResult("version7upgradereport",
                new
                {
                    currentVersion = determinedVersion.ToString(),
                    newVersion = UmbracoVersion.Current.ToString(),
                    errors = CreateReport()
                });
        }

        public override bool RequiresExecution(object model)
        {
            //if it's configured, then no need to run
            if (_runtime.Level == RuntimeLevel.Run)
                return false;

            try
            {
                //we cannot run this step if the db is not configured.
                if (_databaseBuilder.IsDatabaseConfigured == false)
                {
                    return false;
                }
            }
            catch (InvalidOperationException)
            {
                //if there is no db context
                return false;
            }

            var result = _databaseBuilder.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();
            if ((string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false || determinedVersion.Equals(new Version(0, 0, 0)) == false)
                && UmbracoVersion.Current.Major > determinedVersion.Major)
            {
                //it's an upgrade to a major version so we're gonna show this step if there are issues

                var report = CreateReport();
                return report.Any();
            }

            return false;
        }

        private IEnumerable<string> CreateReport()
        {
            var errorReport = new List<string>();

            var sqlSyntax = _databaseContext.SqlSyntax;

            var sql = new Sql();
            sql
                .Select(
                    sqlSyntax.GetQuotedColumn("cmsDataType", "controlId"),
                    sqlSyntax.GetQuotedColumn("umbracoNode", "text"))
                .From(sqlSyntax.GetQuotedTableName("cmsDataType"))
                .InnerJoin(sqlSyntax.GetQuotedTableName("umbracoNode"))
                .On(
                    sqlSyntax.GetQuotedColumn("cmsDataType", "nodeId") + " = " +
                    sqlSyntax.GetQuotedColumn("umbracoNode", "id"));

            List<dynamic> list;
            using (var scope = _scopeProvider.CreateScope())
            {
                list = scope.Database.Fetch<dynamic>(sql);
                scope.Complete();
            }
            foreach (var item in list)
            {
                Guid legacyId = item.controlId;
                //check for a map entry
                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(legacyId);
                if (alias != null)
                {
                    //check that the new property editor exists with that alias
                    var editor = Current.PropertyEditors[alias];
                    if (editor == null)
                    {
                        errorReport.Add($"Property Editor with ID '{item.controlId}' (assigned to Data Type '{item.text}') has a valid GUID -> Alias map but no property editor was found. It will be replaced with a Readonly/Label property editor.");
                    }
                }
                else
                {
                    errorReport.Add($"Property Editor with ID '{item.controlId}' (assigned to Data Type '{item.text}') does not have a valid GUID -> Alias map. It will be replaced with a Readonly/Label property editor.");
                }
            }

            return errorReport;
        }
    }
}
