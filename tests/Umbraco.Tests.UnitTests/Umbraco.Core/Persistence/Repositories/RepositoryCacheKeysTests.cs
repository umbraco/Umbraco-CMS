// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class RepositoryCacheKeysTests
{
    [Test]
    public void GetKey_Returns_Expected_Key_For_Type()
    {
        var key = RepositoryCacheKeys.GetKey<IContent>();
        Assert.AreEqual("uRepo_IContent_", key);
    }

    [Test]
    public void GetKey_Returns_Expected_Key_For_Type_And_Id()
    {
        var key = RepositoryCacheKeys.GetKey<IContent, int>(1000);
        Assert.AreEqual("uRepo_IContent_1000", key);
    }

    /// <summary>
    /// Verifies that the RepositoryCacheKeys.GetKey{T}() method is thread-safe when accessed concurrently with a large
    /// number of unique types.
    /// </summary>
    /// <remarks>
    /// Added as an initially failing test to catch potential thread-safety issues with the internal dictionary, as seen in:
    /// https://github.com/umbraco/Umbraco-CMS/issues/21350
    /// </remarks>
    [Test]
    [LongRunning]
    public void GetKey_Is_ThreadSafe_With_Many_Concurrent_Types()
    {
        // Arrange
        const int ThreadCount = 50;
        const int TypesPerThread = 100;
        var threads = new Thread[ThreadCount];
        var barrier = new ManualResetEventSlim(false);
        var threadSafetyExceptions = new ConcurrentBag<Exception>();

        // Get many unique types to force dictionary growth/resize.
        // Filter out generic type definitions and other problematic types.
        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return [];
                }
            })
            .Where(CanBeUsedAsGenericTypeArgument)
            .Take(ThreadCount * TypesPerThread)
            .ToArray();

        var typeGroups = allTypes
            .Select((t, i) => new { Type = t, Index = i })
            .GroupBy(x => x.Index / TypesPerThread)
            .Select(g => g.Select(x => x.Type).ToArray())
            .ToArray();

        // Act - spawn threads that all call GetKey<T>() concurrently.
        for (int i = 0; i < ThreadCount && i < typeGroups.Length; i++)
        {
            int threadIndex = i;
            threads[i] = new Thread(() =>
            {
                barrier.Wait(); // All threads start together for maximum contention.
                foreach (var type in typeGroups[threadIndex])
                {
                    try
                    {
                        // Use reflection to call GetKey<T>() with runtime type.
                        var method = typeof(RepositoryCacheKeys)
                            .GetMethod(nameof(RepositoryCacheKeys.GetKey), Type.EmptyTypes)!
                            .MakeGenericMethod(type);
                        method.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        var inner = e.InnerException ?? e;

                        // Only collect thread-safety exceptions (concurrent collection modification).
                        // Ignore other exceptions from edge-case types that can't be used as generic arguments.
                        if (inner is InvalidOperationException && inner.Message.Contains("concurrent"))
                        {
                            threadSafetyExceptions.Add(inner);
                        }
                    }
                }
            });
        }

        // Start all threads with suppressed execution context to avoid context leakage.
        using (ExecutionContext.SuppressFlow())
        {
            foreach (var t in threads.Where(t => t is not null))
            {
                t.Start();
            }
        }

        barrier.Set(); // Release all threads simultaneously.
        foreach (var t in threads.Where(t => t is not null))
        {
            t.Join();
        }

        // Assert - only fail on thread-safety violations.
        Assert.IsEmpty(
            threadSafetyExceptions,
            $"Thread safety violation detected: {string.Join(Environment.NewLine, threadSafetyExceptions.Select(e => e.Message))}");
    }

    /// <summary>
    /// Determines whether a type can be used as a generic type argument.
    /// </summary>
    /// <remarks>
    /// Filters out types that cannot be used with <c>MakeGenericMethod</c> or as generic type parameters,
    /// including open generic types, pointer types, by-ref types, ref structs, and void.
    /// </remarks>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type can be used as a generic type argument; otherwise, <c>false</c>.</returns>
    private static bool CanBeUsedAsGenericTypeArgument(Type type)
        => !type.IsGenericTypeDefinition &&
           !type.ContainsGenericParameters &&
           !type.IsPointer &&
           !type.IsByRef &&
           !type.IsByRefLike &&
           type != typeof(void);
}
