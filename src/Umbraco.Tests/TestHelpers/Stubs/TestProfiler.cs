using System;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestProfiler : IProfiler
    {
        public static void Enable()
        {
            _enabled = true;
        }

        public static void Disable()
        {
            _enabled = false;
        }

        private static bool _enabled;

        public string Render()
        {
            return string.Empty;
        }

        public IDisposable Step(string name)
        {
            return _enabled ? MiniProfiler.Current.Step(name) : null;
        }

        public void Start()
        {
            if (_enabled == false) return;

            MiniProfiler.Settings.SqlFormatter = new SqlServerFormatter();
            MiniProfiler.Settings.StackMaxLength = 5000;
            MiniProfiler.Start();
        }

        public void Stop(bool discardResults = false)
        {
            if (_enabled)
                MiniProfiler.Stop(discardResults);
        }
    }
}
