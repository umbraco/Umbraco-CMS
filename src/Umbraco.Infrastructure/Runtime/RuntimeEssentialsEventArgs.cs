using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Runtime
{
    public class RuntimeEssentialsEventArgs : EventArgs
    {
        public RuntimeEssentialsEventArgs(Composition composition, AppCaches appCaches, TypeLoader typeLoader, IUmbracoDatabaseFactory databaseFactory)
        {
            Composition = composition;
            AppCaches = appCaches;
            TypeLoader = typeLoader;
            DatabaseFactory = databaseFactory;
        }

        public Composition Composition { get; }
        public AppCaches AppCaches { get; }
        public TypeLoader TypeLoader { get; }
        public IUmbracoDatabaseFactory DatabaseFactory { get; }
    }
}
