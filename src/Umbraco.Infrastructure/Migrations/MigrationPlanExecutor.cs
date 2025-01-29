using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Org.BouncyCastle.Utilities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/*
 * This is what runs our migration plans.
 * It's important to note that this was altered to allow for partial migration completions.
 * The need for this became apparent when we added support for SQLite.
 * The main issue being how SQLites handles altering the schema.
 * Long story short, SQLite doesn't support altering columns,
 * or adding non-nullable columns with non-trivial types, for instance GUIDs.
 *
 * The recommended workaround for this is to add a new table, migrate the data, and delete the old one,
 * however this causes issues with foreign keys. The recommended approach for this is to disable foreign keys entirely
 * when doing the migration. However, foreign keys MUST be disabled outside a transaction.
 * This was impossible with our previous migration system since it ALWAYS ran all migrations in a single transaction
 * meaning that the transaction will always have been started when your migration is run, so you can't disable FKs.
 *
 * To now support this the UnscopedMigrationBase was added, which allows you to create a migration with no transaction.
 * But this requires each migration to run in its own scope,
 * meaning we can't roll back the entire plan, but only a single migration.
 *
 * Hence the need for partial migration completions.
 */

public class MigrationPlanExecutor : IMigrationPlanExecutor
{
    private readonly ILogger<MigrationPlanExecutor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMigrationBuilder _migrationBuilder;
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;
    private readonly IKeyValueService _keyValueService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AppCaches _appCaches;
    private readonly DistributedCache _distributedCache;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly ICoreScopeProvider _scopeProvider;
    private bool _rebuildCache;
    private bool _invalidateBackofficeUserAccess;

    public MigrationPlanExecutor(
        ICoreScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor,
        ILoggerFactory loggerFactory,
        IMigrationBuilder migrationBuilder,
        IUmbracoDatabaseFactory databaseFactory,
        IDatabaseCacheRebuilder databaseCacheRebuilder,
        DistributedCache distributedCache,
        IKeyValueService keyValueService,
        IServiceScopeFactory serviceScopeFactory,
        AppCaches appCaches)
    {
        _scopeProvider = scopeProvider;
        _scopeAccessor = scopeAccessor;
        _loggerFactory = loggerFactory;
        _migrationBuilder = migrationBuilder;
        _databaseFactory = databaseFactory;
        _databaseCacheRebuilder = databaseCacheRebuilder;
        _keyValueService = keyValueService;
        _serviceScopeFactory = serviceScopeFactory;
        _appCaches = appCaches;
        _distributedCache = distributedCache;
        _logger = _loggerFactory.CreateLogger<MigrationPlanExecutor>();
    }

    [Obsolete("Use the non obsoleted constructor instead. Scheduled for removal in v17")]
    public MigrationPlanExecutor(
        ICoreScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor,
        ILoggerFactory loggerFactory,
        IMigrationBuilder migrationBuilder,
        IUmbracoDatabaseFactory databaseFactory,
        IDatabaseCacheRebuilder databaseCacheRebuilder,
        DistributedCache distributedCache,
        IKeyValueService keyValueService,
        IServiceScopeFactory serviceScopeFactory)
    : this(
        scopeProvider,
        scopeAccessor,
        loggerFactory,
        migrationBuilder,
        databaseFactory,
        databaseCacheRebuilder,
        distributedCache,
        keyValueService,
        serviceScopeFactory,
        StaticServiceProvider.Instance.GetRequiredService<AppCaches>())
    {
    }

    public string Execute(MigrationPlan plan, string fromState) => ExecutePlan(plan, fromState).FinalState;

    /// <summary>
    ///     Executes the plan.
    /// </summary>
    /// <param name="plan">The migration plan to be executes.</param>
    /// <param name="fromState">The state to start execution at.</param>
    /// <returns>ExecutedMigrationPlan containing information about the plan execution, such as completion state and the steps that ran.</returns>
    /// <remarks>
    /// <para>Each migration in the plan, may or may not run in a scope depending on the type of plan.</para>
    /// <para>A plan can complete partially, the changes of each completed migration will be saved.</para>
    /// </remarks>
    public ExecutedMigrationPlan ExecutePlan(MigrationPlan plan, string fromState)
    {
        plan.Validate();

        ExecutedMigrationPlan result = RunMigrationPlan(plan, fromState);

        HandlePostMigrations(result);

        // If any completed migration requires us to rebuild cache we'll do that.
        if (_rebuildCache)
        {
            _logger.LogInformation("Starts rebuilding the cache. This can be a long running operation");
            RebuildCache();
        }

        // If any completed migration requires us to sign out the user we'll do that.
        if (_invalidateBackofficeUserAccess)
        {
            RevokeBackofficeTokens().GetAwaiter().GetResult(); // should async all the way up at some point
        }

        return result;
    }

    [Obsolete]
    private void HandlePostMigrations(ExecutedMigrationPlan result)
    {
        // prepare and de-duplicate post-migrations, only keeping the 1st occurence
        var executedTypes = new HashSet<Type>();

        foreach (IMigrationContext executedMigrationContext in result.ExecutedMigrationContexts)
        {
            if (executedMigrationContext is MigrationContext migrationContext)
            {
                foreach (Type migrationContextPostMigration in migrationContext.PostMigrations)
                {
                    if (executedTypes.Contains(migrationContextPostMigration))
                    {
                        continue;
                    }

                    _logger.LogInformation("PostMigration: {migrationContextFullName}.", migrationContextPostMigration.FullName);
                    MigrationBase postMigration = _migrationBuilder.Build(migrationContextPostMigration, executedMigrationContext);
                    postMigration.Run();

                    executedTypes.Add(migrationContextPostMigration);
                }
            }
        }
    }

    private ExecutedMigrationPlan RunMigrationPlan(MigrationPlan plan, string fromState)
    {
        _logger.LogInformation("Starting '{MigrationName}'...", plan.Name);
        var nextState = fromState;

        _logger.LogInformation("At {OrigState}", string.IsNullOrWhiteSpace(nextState) ? "origin" : nextState);

        if (plan.Transitions.TryGetValue(nextState, out MigrationPlan.Transition? transition) is false)
        {
            plan.ThrowOnUnknownInitialState(nextState);
        }

        List<MigrationPlan.Transition> completedTransitions = new();

        var executedMigrationContexts = new List<IMigrationContext>();
        while (transition is not null)
        {
            _logger.LogInformation("Execute {MigrationType}", transition.MigrationType.Name);

            try
            {
                if (transition.MigrationType.IsAssignableTo(typeof(UnscopedMigrationBase)))
                {
                    executedMigrationContexts.Add(RunUnscopedMigration(transition, plan));
                }
                else
                {
                    executedMigrationContexts.Add(RunScopedMigration(transition, plan));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Plan {PlanName} failed at step {TargetState}", plan.Name, transition.TargetState);

                // We have to always return something, so whatever running this has a chance to save the state we got to.
                return new ExecutedMigrationPlan
                {
                    Successful = false,
                    Exception = exception,
                    InitialState = fromState,
                    FinalState = transition.SourceState,
                    CompletedTransitions = completedTransitions,
                    Plan = plan,
                    ExecutedMigrationContexts = executedMigrationContexts
                };
            }


            IEnumerable<IMigrationContext> nonCompletedMigrationsContexts = executedMigrationContexts.Where(x => x.IsCompleted is false);
            if (nonCompletedMigrationsContexts.Any())
            {
                throw new InvalidOperationException($"Migration ({transition.MigrationType.FullName}) has been executed without indicated it was completed correctly.");
            }

            // The plan migration (transition), completed, so we'll add this to our list so we can return this at some point.
            completedTransitions.Add(transition);
            nextState = transition.TargetState;

            _logger.LogInformation("At {OrigState}", nextState);

            // this should never happen as the plan has been validated - this is just a paranoid safety test
            // If it does something is wrong and we'll fail the execution and return an error.
            if (plan.Transitions.TryGetValue(nextState, out transition) is false)
            {
                return new ExecutedMigrationPlan
                {
                    Successful = false,
                    Exception = new InvalidOperationException($"Unknown state \"{nextState}\"."),
                    InitialState = fromState,
                    // We were unable to get the next transition, and we never executed it, so final state is source state.
                    FinalState = completedTransitions.Last().TargetState,
                    CompletedTransitions = completedTransitions,
                    Plan = plan,
                };
            }
        }

        _logger.LogInformation("Done");

        // final state is set to either the transition target state
        // or the final completed transition target state if transition is null
        // or the original migration state, if no transitions completed
        string finalState = fromState;
        if (transition is not null)
        {
            finalState = transition.TargetState;
        }
        else if (completedTransitions.Any())
        {
            finalState = completedTransitions.Last().TargetState;
        }

        return new ExecutedMigrationPlan
        {
            Successful = true,
            InitialState = fromState,
            FinalState = finalState,
            CompletedTransitions = completedTransitions,
            Plan = plan,
            ExecutedMigrationContexts = executedMigrationContexts
        };
    }

    private MigrationContext RunUnscopedMigration(MigrationPlan.Transition transition, MigrationPlan plan)
    {
        using IUmbracoDatabase database = _databaseFactory.CreateDatabase();
        var context = new MigrationContext(plan, database, _loggerFactory.CreateLogger<MigrationContext>(), () => OnComplete(plan, transition.TargetState));

        RunMigration(transition.MigrationType, context);

        return context;
    }

    private void OnComplete(MigrationPlan plan, string targetState)
    {
        _keyValueService.SetValue(Constants.Conventions.Migrations.KeyValuePrefix + plan.Name, targetState);
    }

    private MigrationContext RunScopedMigration(MigrationPlan.Transition transition, MigrationPlan plan)
    {
        // We want to suppress scope (service, etc...) notifications during a migration plan
        // execution. This is because if a package that doesn't have their migration plan
        // executed is listening to service notifications to perform some persistence logic,
        // that packages notification handlers may explode because that package isn't fully installed yet.
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        using (scope.Notifications.Suppress())
        {
            var context = new MigrationContext(
                plan,
                _scopeAccessor.AmbientScope?.Database,
                _loggerFactory.CreateLogger<MigrationContext>(),
                () => OnComplete(plan, transition.TargetState));

            RunMigration(transition.MigrationType, context);

            // Ensure we mark the context as complete before the scope completes
            context.Complete();

            scope.Complete();

            return context;
        }
    }

    private void RunMigration(Type migrationType, MigrationContext context)
    {
        MigrationBase migration = _migrationBuilder.Build(migrationType, context);
        migration.Run();

        // If the migration requires clearing the cache set the flag, this will automatically only happen if it succeeds
        // Otherwise it'll error out before and return.
        if (migration.RebuildCache)
        {
            _rebuildCache = true;
        }

        if (migration.InvalidateBackofficeUserAccess)
        {
            _invalidateBackofficeUserAccess = true;
        }
    }

    private void RebuildCache()
    {
        _appCaches.RuntimeCache.Clear();
        _appCaches.IsolatedCaches.ClearAllCaches();
        _databaseCacheRebuilder.Rebuild();
        _distributedCache.RefreshAllPublishedSnapshot();
    }

    private async Task RevokeBackofficeTokens()
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IOpenIddictApplicationManager openIddictApplicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var backOfficeClient = await openIddictApplicationManager.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice);
        if (backOfficeClient is null)
        {
            _logger.LogWarning("Could not get the openIddict Application for {backofficeClientId}. Canceling token revocation. Users might have to manually log out to get proper access to the backoffice", Constants.OAuthClientIds.BackOffice);
            return;
        }

        var backOfficeClientId = await openIddictApplicationManager.GetIdAsync(backOfficeClient);
        if (backOfficeClientId is null)
        {
            _logger.LogWarning("Could not extract the clientId from the openIddict backofficelient Application. Canceling token revocation. Users might have to manually log out to get proper access to the backoffice", Constants.OAuthClientIds.BackOffice);
            return;
        }

        IOpenIddictTokenManager tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var tokens = await tokenManager.FindByApplicationIdAsync(backOfficeClientId).ToArrayAsync();
        foreach (var token in tokens)
        {
            await tokenManager.DeleteAsync(token);
        }
    }
}
