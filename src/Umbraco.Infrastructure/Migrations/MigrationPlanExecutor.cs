using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using Type = System.Type;

namespace Umbraco.Cms.Infrastructure.Migrations
{
    public class MigrationPlanExecutor : IMigrationPlanExecutor
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly ILogger<MigrationPlanExecutor> _logger;

        public MigrationPlanExecutor(IScopeProvider scopeProvider, ILoggerFactory loggerFactory, IMigrationBuilder migrationBuilder)
        {
            _scopeProvider = scopeProvider;
            _loggerFactory = loggerFactory;
            _migrationBuilder = migrationBuilder;
            _logger = _loggerFactory.CreateLogger<MigrationPlanExecutor>();
        }

        /// <summary>
        /// Executes the plan.
        /// </summary>
        /// <param name="scope">A scope.</param>
        /// <param name="fromState">The state to start execution at.</param>
        /// <param name="migrationBuilder">A migration builder.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="loggerFactory"></param>
        /// <returns>The final state.</returns>
        /// <remarks>The plan executes within the scope, which must then be completed.</remarks>
        public string Execute(MigrationPlan plan, string fromState)
        {
            plan.Validate();

            _logger.LogInformation("Starting '{MigrationName}'...", plan.Name);

            var origState = fromState ?? string.Empty;

            _logger.LogInformation("At {OrigState}", string.IsNullOrWhiteSpace(origState) ? "origin" : origState);

            if (!plan.Transitions.TryGetValue(origState, out MigrationPlan.Transition transition))
            {
                plan.ThrowOnUnknownInitialState(origState);
            }

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var context = new MigrationContext(scope.Database, _loggerFactory.CreateLogger<MigrationContext>());
                context.PostMigrations.AddRange(plan.PostMigrationTypes);

                while (transition != null)
                {
                    _logger.LogInformation("Execute {MigrationType}", transition.MigrationType.Name);

                    var migration = _migrationBuilder.Build(transition.MigrationType, context);
                    migration.Migrate();

                    var nextState = transition.TargetState;
                    origState = nextState;

                    _logger.LogInformation("At {OrigState}", origState);

                    // throw a raw exception here: this should never happen as the plan has
                    // been validated - this is just a paranoid safety test
                    if (!plan.Transitions.TryGetValue(origState, out transition))
                    {
                        throw new InvalidOperationException($"Unknown state \"{origState}\".");
                    }
                }

                // prepare and de-duplicate post-migrations, only keeping the 1st occurence
                var temp = new HashSet<Type>();
                var postMigrationTypes = context.PostMigrations
                    .Where(x => !temp.Contains(x))
                    .Select(x => { temp.Add(x); return x; });

                // run post-migrations
                foreach (var postMigrationType in postMigrationTypes)
                {
                    _logger.LogInformation($"PostMigration: {postMigrationType.FullName}.");
                    var postMigration = _migrationBuilder.Build(postMigrationType, context);
                    postMigration.Migrate();
                }
            }

            _logger.LogInformation("Done (pending scope completion).");

            // safety check - again, this should never happen as the plan has been validated,
            // and this is just a paranoid safety test
            var finalState = plan.FinalState;
            if (origState != finalState)
            {
                throw new InvalidOperationException($"Internal error, reached state {origState} which is not final state {finalState}");
            }

            return origState;
        }
    }
}
