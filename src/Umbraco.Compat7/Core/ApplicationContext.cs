using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
    public class ApplicationContext : IDisposable
    {
        private ApplicationContext()
        {
            DatabaseContext = new DatabaseContext(DI.Current.Container.GetInstance<IUmbracoDatabaseFactory>());
        }

        public static ApplicationContext Current { get; } = new ApplicationContext();

        public CacheHelper ApplicationCache => DI.Current.ApplicationCache;

        public ProfilingLogger ProfilingLogger => DI.Current.ProfilingLogger;

        public bool IsReady { get; } = true; // because... not accessible before we are ready

        public bool IsConfigured => DI.Current.RuntimeState.Level == RuntimeLevel.Run;

	    public bool IsUpgrading => DI.Current.RuntimeState.Level == RuntimeLevel.Upgrade;

        public DatabaseContext DatabaseContext { get; }

        public ServiceContext Services => DI.Current.Services;

        public void Dispose()
        { }
    }
}
