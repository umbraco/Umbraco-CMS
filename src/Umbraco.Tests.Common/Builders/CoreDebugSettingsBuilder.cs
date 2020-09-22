using CoreDebugSettings = Umbraco.Core.Configuration.Models.CoreDebugSettings;

namespace Umbraco.Tests.Common.Builders
{
    public class CoreDebugSettingsBuilder : BuilderBase<CoreDebugSettings>
    {
        private bool? _dumpOnTimeoutThreadAbort;
        private bool? _logUncompletedScopes;

        public CoreDebugSettingsBuilder WithDumpOnTimeoutThreadAbort(bool dumpOnTimeoutThreadAbort)
        {
            _dumpOnTimeoutThreadAbort = dumpOnTimeoutThreadAbort;
            return this;
        }

        public CoreDebugSettingsBuilder WithLogUncompletedScopes(bool logUncompletedScopes)
        {
            _logUncompletedScopes = logUncompletedScopes;
            return this;
        }

        public override CoreDebugSettings Build()
        {
            var dumpOnTimeoutThreadAbort = _dumpOnTimeoutThreadAbort ?? false;
            var logUncompletedScopes = _logUncompletedScopes ?? false;

            return new CoreDebugSettings
            {
                DumpOnTimeoutThreadAbort = dumpOnTimeoutThreadAbort,
                LogUncompletedScopes = logUncompletedScopes,
            };
        }
    }
}
