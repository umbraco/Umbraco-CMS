// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

internal static class JitCompilationAssert
{
    /// <summary>
    ///     Asserts that repeatedly running an action does not JIT-compile a new method on each run.
    /// </summary>
    public static void DoesNotCompilePerRun(Action<int> run, int iterations = 200)
    {
        run(0);
        var compiledBefore = JitInfo.GetCompiledMethodCount(currentThread: true);

        for (var i = 0; i < iterations; i++)
        {
            run(i);
        }

        var compiled = JitInfo.GetCompiledMethodCount(currentThread: true) - compiledBefore;
        Assert.Less(compiled, iterations, $"{compiled} methods were JIT-compiled over {iterations} runs.");
    }
}
