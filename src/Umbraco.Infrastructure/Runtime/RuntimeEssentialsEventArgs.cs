using System;
using Umbraco.Core.Builder;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Runtime
{
    public class RuntimeEssentialsEventArgs : EventArgs
    {
        public RuntimeEssentialsEventArgs(IUmbracoBuilder builder, IUmbracoDatabaseFactory databaseFactory)
        {
            Builder = builder;
            DatabaseFactory = databaseFactory;
        }

        public IUmbracoBuilder Builder { get; }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }
    }
}
