using System.Diagnostics;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Tests.Search.Integration;

public abstract class UmbracoIntegrationTestWithPackageMigrations : UmbracoIntegrationTest
{
    // NOTE: This works because NUnit calls base class SetUp methods before those in the derived classes.
    //       See https://docs.nunit.org/articles/nunit/writing-tests/attributes/setup.html
    [SetUp]
    public async Task SetupAsync() => await WaitForPackageMigrationsAsync();

    /// <summary>
    /// Package migrations now run automatically on a background hosted service, so this waits for
    /// <see cref="IRuntimeState.Level"/> to reach <see cref="RuntimeLevel.Run"/> instead of running them explicitly.
    /// </summary>
    private async Task WaitForPackageMigrationsAsync()
    {
        IRuntimeState runtimeState = GetRequiredService<IRuntimeState>();
        var stopWatch = Stopwatch.StartNew();

        while (runtimeState.Level != RuntimeLevel.Run)
        {
            if (runtimeState.Level == RuntimeLevel.BootFailed)
            {
                throw new InvalidOperationException("Runtime boot failed while waiting for package migrations to run.", runtimeState.BootFailedException);
            }

            if (stopWatch.ElapsedMilliseconds > 30000)
            {
                throw new TimeoutException($"Timed out waiting for package migrations to complete (RuntimeState.Level is currently {runtimeState.Level}).");
            }

            await Task.Delay(250);
        }

        // one final await, because there is a small window where, even though we are now running, we can still lock the database.
        await Task.Delay(500);
    }
}
