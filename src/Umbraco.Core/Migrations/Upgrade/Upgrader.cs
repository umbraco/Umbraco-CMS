using System;
using Semver;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    public abstract class Upgrader
    {
        private readonly IKeyValueService _keyValueService;
        private readonly PostMigrationCollection _postMigrations;

        protected Upgrader(string name, IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, PostMigrationCollection postMigrations, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;
            StateValueKey = "Umbraco.Core.Upgrader.State+" + name;

            ScopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            MigrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            _postMigrations = postMigrations ?? throw new ArgumentNullException(nameof(postMigrations));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Name { get; }

        public string StateValueKey { get; }

        protected IScopeProvider ScopeProvider { get; }

        protected IMigrationBuilder MigrationBuilder { get; }

        protected ILogger Logger { get; }

        protected abstract MigrationPlan GetPlan();
        protected abstract string GetInitialState();
        protected abstract (SemVersion, SemVersion) GetVersions();

        public void Execute()
        {
            var plan = GetPlan();

            using (var scope = ScopeProvider.CreateScope())
            {
                // read current state
                var currentState = _keyValueService.GetValue(StateValueKey) ?? string.Empty;
                var forceState = false;

                if (currentState == string.Empty)
                {
                    currentState = GetInitialState();
                    forceState = true;
                }

                (var originVersion, var targetVersion) = GetVersions();

                // execute plan
                var state = plan.Execute(scope, currentState);

                // save new state
                if (forceState)
                    _keyValueService.SetValue(StateValueKey, state);
                else
                    _keyValueService.SetValue(StateValueKey, currentState, state);

                // run post-migrations
                foreach (var postMigration in _postMigrations)
                    postMigration.Execute(Name, scope, originVersion, targetVersion, Logger);

                scope.Complete();
            }
        }
    }
}
