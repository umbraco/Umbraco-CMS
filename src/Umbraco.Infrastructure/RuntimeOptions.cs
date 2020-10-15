using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides static options for the runtime.
    /// </summary>
    /// <remarks>
    /// These options can be configured in PreApplicationStart or via appSettings.
    /// </remarks>
    public static class RuntimeOptions
    {
        private static List<Action<IProfilingLogger>> _onBoot;
        private static List<Action<Composition, AppCaches, TypeLoader, IUmbracoDatabaseFactory>> _onEssentials;

        /// <summary>
        /// Executes the RuntimeBoot handlers.
        /// </summary>
        internal static void DoRuntimeBoot(IProfilingLogger logger)
        {
            if (_onBoot == null)
                return;

            foreach (var action in _onBoot)
                action(logger);
        }

        /// <summary>
        /// Executes the RuntimeEssentials handlers.
        /// </summary>
        internal static void DoRuntimeEssentials(Composition composition, AppCaches appCaches, TypeLoader typeLoader, IUmbracoDatabaseFactory databaseFactory)
        {
            if (_onEssentials== null)
                return;

            foreach (var action in _onEssentials)
                action(composition, appCaches, typeLoader, databaseFactory);
        }

        /// <summary>
        /// Registers a RuntimeBoot handler.
        /// </summary>
        /// <remarks>
        /// <para>A RuntimeBoot handler runs when the runtime boots, right after the
        /// loggers have been created, but before anything else.</para>
        /// </remarks>
        public static void OnRuntimeBoot(Action<IProfilingLogger> action)
        {
            if (_onBoot == null)
                _onBoot = new List<Action<IProfilingLogger>>();
            _onBoot.Add(action);
        }

        /// <summary>
        /// Registers a RuntimeEssentials handler.
        /// </summary>
        /// <remarks>
        /// <para>A RuntimeEssentials handler runs after the runtime has created a few
        /// essential things (AppCaches, a TypeLoader, and a database factory) but
        /// before anything else.</para>
        /// </remarks>
        public static void OnRuntimeEssentials(Action<Composition, AppCaches, TypeLoader, IUmbracoDatabaseFactory> action)
        {
            if (_onEssentials == null)
                _onEssentials = new List<Action<Composition, AppCaches, TypeLoader, IUmbracoDatabaseFactory>>();
            _onEssentials.Add(action);
        }
    }
}
