using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class EFCoreMigrationPlanExecutor : IEFCoreMigrationPlanExecutor
{
    private readonly ILogger<EFCoreMigrationPlanExecutor> _logger;
    private readonly IEFCoreMigrationBuilder _efCoreMigrationBuilder;

    public EFCoreMigrationPlanExecutor(ILogger<EFCoreMigrationPlanExecutor> logger, IEFCoreMigrationBuilder efCoreMigrationBuilder)
    {
        _logger = logger;
        _efCoreMigrationBuilder = efCoreMigrationBuilder;
    }

    public ExecutedEFCoreMigrationPlan ExecutePlan(EFCoreMigrationPlan plan, string fromState)
    {
        plan.Validate();

        _logger.LogInformation("Starting '{MigrationName}'...", plan.Name);

        var nextState = fromState;

        _logger.LogInformation("At {OrigState}", string.IsNullOrWhiteSpace(nextState) ? "origin" : nextState);

        if (!plan.Transitions.TryGetValue(nextState, out Transition? transition))
        {
            throw new InvalidOperationException(
                $"The migration plan does not support migrating from state \"{nextState}\".");
        }

        var context = new EFCoreMigrationContext(plan);

        while (transition != null)
        {
            _logger.LogInformation("Execute {MigrationType}", transition.MigrationType.Name);

            EfCoreMigrationBase migration = _efCoreMigrationBuilder.Build(transition.MigrationType, context);
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

        // safety check - again, this should never happen as the plan has been validated,
        // and this is just a paranoid safety test
        var finalState = plan.FinalState;
        if (nextState != finalState)
        {
            throw new InvalidOperationException(
                $"Internal error, reached state {nextState} which is not final state {finalState}");
        }

        return new ExecutedEFCoreMigrationPlan(plan, fromState, nextState);
    }
}
