using System;
using Semver;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    public abstract class Upgrader
    {
        private readonly IKeyValueService _keyValueService;
        private readonly PostMigrationCollection _postMigrations;
        private MigrationPlan _plan;

        protected Upgrader(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, PostMigrationCollection postMigrations, ILogger logger)
        {
            ScopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            MigrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            _postMigrations = postMigrations ?? throw new ArgumentNullException(nameof(postMigrations));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Name => Plan.Name;

        public string StateValueKey => GetStateValueKey(Plan);

        protected IScopeProvider ScopeProvider { get; }

        protected IMigrationBuilder MigrationBuilder { get; }

        protected ILogger Logger { get; }

        protected MigrationPlan Plan => _plan ?? (_plan = GetPlan());

        protected abstract MigrationPlan GetPlan();
        protected abstract (SemVersion, SemVersion) GetVersions();

        public void Execute()
        {
            var plan = Plan;

            using (var scope = ScopeProvider.CreateScope())
            {
                // read current state
                var currentState = _keyValueService.GetValue(StateValueKey);
                var forceState = false;

                if (currentState == null)
                {
                    currentState = plan.InitialState;
                    forceState = true;
                }

                // execute plan
                var state = plan.Execute(scope, currentState);
                if (string.IsNullOrWhiteSpace(state))
                    throw new Exception("Plan execution returned an invalid null or empty state.");

                // save new state
                if (forceState)
                    _keyValueService.SetValue(StateValueKey, state);
                else if (currentState != state)
                    _keyValueService.SetValue(StateValueKey, currentState, state);

                // run post-migrations
                (var originVersion, var targetVersion) = GetVersions();
                foreach (var postMigration in _postMigrations)
                    postMigration.Execute(Name, scope, originVersion, targetVersion, Logger);

                scope.Complete();
            }
        }

        public static string GetStateValueKey(MigrationPlan plan) => "Umbraco.Core.Upgrader.State+" + plan.Name;
    }
}
