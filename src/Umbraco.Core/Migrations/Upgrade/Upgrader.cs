using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    /// <summary>
    /// Represents an upgrader.
    /// </summary>
    public class Upgrader
    {
        /// <summary>
        /// Initializes a new instance of the <see ref="Upgrader"/> class.
        /// </summary>
        public Upgrader(MigrationPlan plan)
        {
            Plan = plan;
        }

        /// <summary>
        /// Gets the name of the migration plan.
        /// </summary>
        public string Name => Plan.Name;

        /// <summary>
        /// Gets the migration plan.
        /// </summary>
        public MigrationPlan Plan { get; }

        /// <summary>
        /// Gets the key for the state value.
        /// </summary>
        public virtual string StateValueKey => "Umbraco.Core.Upgrader.State+" + Name;

        /// <summary>
        /// Executes.
        /// </summary>
        /// <param name="scopeProvider">A scope provider.</param>
        /// <param name="migrationBuilder">A migration builder.</param>
        /// <param name="keyValueService">A key-value service.</param>
        /// <param name="logger">A logger.</param>
        public void Execute(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger)
        {
            if (scopeProvider == null) throw new ArgumentNullException(nameof(scopeProvider));
            if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));
            if (keyValueService == null) throw new ArgumentNullException(nameof(keyValueService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            var plan = Plan;

            using (var scope = scopeProvider.CreateScope())
            {
                BeforeMigrations(scope, logger);

                // read current state
                var currentState = keyValueService.GetValue(StateValueKey);
                var forceState = false;

                if (currentState == null)
                {
                    currentState = plan.InitialState;
                    forceState = true;
                }

                // execute plan
                var state = plan.Execute(scope, currentState, migrationBuilder, logger);
                if (string.IsNullOrWhiteSpace(state))
                    throw new Exception("Plan execution returned an invalid null or empty state.");

                // save new state
                if (forceState)
                    keyValueService.SetValue(StateValueKey, state);
                else if (currentState != state)
                    keyValueService.SetValue(StateValueKey, currentState, state);

                AfterMigrations(scope, logger);

                scope.Complete();
            }
        }

        /// <summary>
        /// Executes as part of the upgrade scope and before all migrations have executed.
        /// </summary>
        public virtual void BeforeMigrations(IScope scope, ILogger logger)
        { }

        /// <summary>
        /// Executes as part of the upgrade scope and after all migrations have executed.
        /// </summary>
        public virtual void AfterMigrations(IScope scope, ILogger logger)
        { }

    }
}
