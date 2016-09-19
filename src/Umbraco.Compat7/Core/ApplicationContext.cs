using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
    public class ApplicationContext : IDisposable
    {
        private ApplicationContext()
        { }

        public static ApplicationContext Current { get; } = new ApplicationContext();

        public CacheHelper ApplicationCache => DependencyInjection.Current.ApplicationCache;

        public ProfilingLogger ProfilingLogger => DependencyInjection.Current.ProfilingLogger;

        public bool IsReady { get; } = true; // because... not accessible before we are ready

        public bool IsConfigured => DependencyInjection.Current.RuntimeState.Level == RuntimeLevel.Run;

	    public bool IsUpgrading => DependencyInjection.Current.RuntimeState.Level == RuntimeLevel.Upgrade;

        public DatabaseContext DatabaseContext => DependencyInjection.Current.DatabaseContext;

        public ServiceContext Services => DependencyInjection.Current.Services;

        public void Dispose()
        { }
    }
}
