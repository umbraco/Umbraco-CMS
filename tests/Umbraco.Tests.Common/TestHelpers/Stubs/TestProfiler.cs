// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.Common.TestHelpers.Stubs;

public class TestProfiler : IProfiler
{
    private static bool s_enabled;

    public IDisposable Step(string name) => s_enabled ? MiniProfiler.Current.Step(name) : null;

    public void Start()
    {
        if (s_enabled == false)
        {
            return;
        }

        // See https://miniprofiler.com/dotnet/AspDotNet
        MiniProfiler.Configure(new MiniProfilerOptions
        {
            SqlFormatter = new SqlServerFormatter(),
            StackMaxLength = 5000
        });

        MiniProfiler.StartNew();
    }

    public void Stop(bool discardResults = false)
    {
        if (s_enabled)
        {
            MiniProfiler.Current.Stop(discardResults);
        }
    }

    public static void Enable() => s_enabled = true;

    public static void Disable() => s_enabled = false;
}
