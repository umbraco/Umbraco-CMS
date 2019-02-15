using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
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
        private static bool? _installMissingDatabase;
        private static bool? _installEmptyDatabase;

        // reads a boolean appSetting
        private static bool BoolSetting(string key, bool missing) => ConfigurationManager.AppSettings[key]?.InvariantEquals("true") ?? missing;

        /// <summary>
        /// Gets a value indicating whether the runtime should enter Install level when the database is missing.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured but it is not possible to
        /// connect to the database, the runtime enters the BootFailed level. If this options is set to true,
        /// it enters the Install level instead.</para>
        /// <para>It is then up to the implementor, that is setting this value, to take over the installation
        /// sequence.</para>
        /// </remarks>
        public static bool InstallMissingDatabase
        {
            get => _installEmptyDatabase ?? BoolSetting("Umbraco.Core.RuntimeState.InstallMissingDatabase", false);
            set => _installEmptyDatabase = value;
        }

        /// <summary>
        /// Gets a value indicating whether the runtime should enter Install level when the database is empty.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured and it is possible to connect to
        /// the database, but the database is empty, the runtime enters the BootFailed level. If this options
        /// is set to true, it enters the Install level instead.</para>
        /// <para>It is then up to the implementor, that is setting this value, to take over the installation
        /// sequence.</para>
        /// </remarks>
        public static bool InstallEmptyDatabase
        {
            get => _installMissingDatabase ?? BoolSetting("Umbraco.Core.RuntimeState.InstallEmptyDatabase", false);
            set => _installMissingDatabase = value;
        }

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
