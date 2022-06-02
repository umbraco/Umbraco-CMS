using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class MigrationPlanExecutor : IMigrationPlanExecutor
{
    private readonly ILogger<MigrationPlanExecutor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMigrationBuilder _migrationBuilder;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly ICoreScopeProvider _scopeProvider;

    public MigrationPlanExecutor(
        ICoreScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor,
        ILoggerFactory loggerFactory,
        IMigrationBuilder migrationBuilder)
    {
        _scopeProvider = scopeProvider;
        _scopeAccessor = scopeAccessor;
        _loggerFactory = loggerFactory;
        _migrationBuilder = migrationBuilder;
        _logger = _loggerFactory.CreateLogger<MigrationPlanExecutor>();
    }

    /// <summary>
    ///     Executes the plan.
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

        fromState ??= string.Empty;
        var nextState = fromState;

        _logger.LogInformation("At {OrigState}", string.IsNullOrWhiteSpace(nextState) ? "origin" : nextState);

        if (!plan.Transitions.TryGetValue(nextState, out MigrationPlan.Transition? transition))
        {
            plan.ThrowOnUnknownInitialState(nextState);
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            // We want to suppress scope (service, etc...) notifications during a migration plan
            // execution. This is because if a package that doesn't have their migration plan
            // executed is listening to service notifications to perform some persistence logic,
            // that packages notification handlers may explode because that package isn't fully installed yet.
            using (scope.Notifications.Suppress())
            {
                var context = new MigrationContext(plan, _scopeAccessor.AmbientScope?.Database,
                    _loggerFactory.CreateLogger<MigrationContext>());

                while (transition != null)
                {
                    _logger.LogInformation("Execute {MigrationType}", transition.MigrationType.Name);

                    MigrationBase migration = _migrationBuilder.Build(transition.MigrationType, context);
                    migration.Run();

                    nextState = transition.TargetState;

                    _logger.LogInformation("At {OrigState}", nextState);

                    // throw a raw exception here: this should never happen as the plan has
                    // been validated - this is just a paranoid safety test
                    if (!plan.Transitions.TryGetValue(nextState, out transition))
                    {
                        throw new InvalidOperationException($"Unknown state \"{nextState}\".");
                    }
                }

                // prepare and de-duplicate post-migrations, only keeping the 1st occurence
                var temp = new HashSet<Type>();
                IEnumerable<Type> postMigrationTypes = context.PostMigrations
                    .Where(x => !temp.Contains(x))
                    .Select(x =>
                    {
                        temp.Add(x);
                        return x;
                    });

                // run post-migrations
                foreach (Type postMigrationType in postMigrationTypes)
                {
                    _logger.LogInformation($"PostMigration: {postMigrationType.FullName}.");
                    MigrationBase postMigration = _migrationBuilder.Build(postMigrationType, context);
                    postMigration.Run();
                }
            }
        }

        _logger.LogInformation("Done (pending scope completion).");

        // safety check - again, this should never happen as the plan has been validated,
        // and this is just a paranoid safety test
        var finalState = plan.FinalState;
        if (nextState != finalState)
        {
            throw new InvalidOperationException(
                $"Internal error, reached state {nextState} which is not final state {finalState}");
        }

        return nextState;
    }
}
