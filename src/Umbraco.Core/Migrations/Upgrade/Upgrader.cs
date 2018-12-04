using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    /// <summary>
    /// Provides a base class for upgraders.
    /// </summary>
    public abstract class Upgrader
    {
        private readonly IKeyValueService _keyValueService;
        private MigrationPlan _plan;

        /// <summary>
        /// Initializes a new instance of the <see cref="Upgrader"/> class.
        /// </summary>
        protected Upgrader(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger)
        {
            ScopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            MigrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the name of the migration plan.
        /// </summary>
        public string Name => Plan.Name;

        /// <summary>
        /// Gets the state value key corresponding to the migration plan.
        /// </summary>
        public string StateValueKey => GetStateValueKey(Plan);

        /// <summary>
        /// Gets the scope provider.
        /// </summary>
        protected IScopeProvider ScopeProvider { get; }

        /// <summary>
        /// Gets the migration builder.
        /// </summary>
        protected IMigrationBuilder MigrationBuilder { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the migration plan.
        /// </summary>
        protected MigrationPlan Plan => _plan ?? (_plan = CreatePlan());

        /// <summary>
        /// Creates the migration plan.
        /// </summary>
        /// <returns></returns>
        protected abstract MigrationPlan CreatePlan();

        /// <summary>
        /// Executes.
        /// </summary>
        public void Execute()
        {
            var plan = Plan;

            using (var scope = ScopeProvider.CreateScope())
            {
                BeforeMigrations(scope);

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

                AfterMigrations(scope);

                scope.Complete();
            }
        }

        /// <summary>
        /// Executes as part of the upgrade scope and before all migrations have executed.
        /// </summary>
        public virtual void BeforeMigrations(IScope scope)
        { }

        /// <summary>
        /// Executes as part of the upgrade scope and after all migrations have executed.
        /// </summary>
        public virtual void AfterMigrations(IScope scope)
        { }

        /// <summary>
        /// Gets the state value key for a migration plan.
        /// </summary>
        public static string GetStateValueKey(MigrationPlan plan) => "Umbraco.Core.Upgrader.State+" + plan.Name;
    }
}
