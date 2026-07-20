// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Security;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    WithApplication = true)]
internal sealed class MemberUserStoreConcurrencyTests : UmbracoIntegrationTest
{
    private IMemberUserStore MemberUserStore => GetRequiredService<IMemberUserStore>();

    /// <summary>
    /// Simulates concurrent member auto-link registrations as would occur
    /// when multiple users sign in via an external provider simultaneously.
    /// Each auto-link performs CreateAsync followed by UpdateAsync (security stamp).
    /// On SQLite, if read and write operations share a transaction scope,
    /// concurrent requests contend for the write lock and fail with SQLITE_LOCKED.
    /// </summary>
    [Explicit]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Concurrent_Member_AutoLink_Should_Not_Cause_Database_Lock_Errors(bool externalOnly)
    {
        const int concurrentOperations = 5;
        var exceptions = new ConcurrentBag<Exception>();
        var barrier = new Barrier(concurrentOperations);

        var tasks = new List<Task>();
        for (var i = 0; i < concurrentOperations; i++)
        {
            var index = i;
            using (ExecutionContext.SuppressFlow())
            {
                tasks.Add(Task.Run(async () =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        await SimulateMemberAutoLinkAsync(index, externalOnly);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }
        }

        // Allow up to 30 seconds — if operations are blocked by SQLite locks,
        // they'll either fail with SQLITE_LOCKED or still be waiting when time expires.
        var completedInTime = await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(30))
            .ContinueWith(t => t.IsCompletedSuccessfully);

        // Build a combined failure message from lock errors and/or timed-out operations.
        var failures = new List<string>(exceptions.Select(e => e.Message));
        if (completedInTime is false)
        {
            var pendingCount = tasks.Count(t => !t.IsCompleted);
            failures.Add($"{pendingCount} operation(s) still blocked on database lock after 30 seconds");
        }

        Assert.IsEmpty(failures, string.Join("\n", failures));
    }

    /// <summary>
    /// Simulates the auto-link flow that MemberSignInManager performs:
    /// 1. Create the member via UserStore.CreateAsync.
    /// 2. Update the member via UserStore.UpdateAsync (as happens during sign-in for security stamp).
    /// </summary>
    private async Task SimulateMemberAutoLinkAsync(int index, bool externalOnly)
    {
        // Use a short cancellation timeout so blocked operations fail fast
        // instead of waiting for the full SQLite busy timeout (~30s per operation).
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var email = $"concurrent-{index}@test.com";
        var user = MemberIdentityUser.CreateNew(
            email,
            email,
            Constants.Conventions.MemberTypes.DefaultAlias,
            isApproved: true,
            name: $"Concurrent Test {index}");
        user.IsExternalOnly = externalOnly;

        // Step 1: Create (as MemberSignInManager.AutoLinkAndSignInExternalAccount does).
        IdentityResult createResult = await MemberUserStore.CreateAsync(user, cts.Token);
        if (createResult.Succeeded is false)
        {
            throw new InvalidOperationException(
                $"CreateAsync failed for user {index}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        // Step 2: Update security stamp (as SignInOrTwoFactorAsync does after sign-in).
        user.SecurityStamp = Guid.NewGuid().ToString();
        IdentityResult updateResult = await MemberUserStore.UpdateAsync(user, cts.Token);
        if (updateResult.Succeeded is false)
        {
            throw new InvalidOperationException(
                $"UpdateAsync failed for user {index}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
        }
    }
}
