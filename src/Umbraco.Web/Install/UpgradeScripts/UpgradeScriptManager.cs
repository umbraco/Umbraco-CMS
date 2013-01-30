using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using umbraco.DataLayer.Utility.Installer;

namespace Umbraco.Web.Install.UpgradeScripts
{
    /// <summary>
    /// Class used to register and execute upgrade scripts during install if they are required.
    /// </summary>
    internal static class UpgradeScriptManager
    {
        /// <summary>
        /// Returns true if there are scripts to execute for the version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool HasScriptsForVersion(Version version)
        {
            return Scripts.Any(x => x.Item2.InRange(version));
        }

        /// <summary>
        /// Executes all of the scripts for a database version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static void ExecuteScriptsForVersion(Version version)
        {
            var types = Scripts.Where(x => x.Item2.InRange(version)).Select(x => x.Item1);
            foreach (var instance in types.Select(x => x()))
            {
                instance.Execute();
            }
        }

        public static void AddUpgradeScript(Func<IUpgradeScript> script, VersionRange version)
        {
            Scripts.Add(new Tuple<Func<IUpgradeScript>, VersionRange>(script, version));
        }

        ///// <summary>
        ///// Adds a script to execute for a database version
        ///// </summary>
        ///// <param name="assemblyQualifiedTypeName"></param>
        ///// <param name="version"></param>
        //public static void AddUpgradeScript(string assemblyQualifiedTypeName, VersionRange version)
        //{
        //    AddUpgradeScript(new Lazy<Type>(() => Type.GetType(assemblyQualifiedTypeName)), version);
        //}

        ///// <summary>
        ///// Adds a script to execute for a database version
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="version"></param>
        //public static void AddUpgradeScript<T>(VersionRange version)
        //{
        //    AddUpgradeScript(new Lazy<Type>(() => typeof(T)), version);
        //}

        /// <summary>
        /// Used for testing
        /// </summary>
        internal static void Clear()
        {
            Scripts.Clear();
        }

        ///// <summary>
        ///// Adds a script to execute for a database version
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="version"></param>
        //public static void AddUpgradeScript(Lazy<Type> type, VersionRange version)
        //{
        //    Scripts.Add(new Tuple<Lazy<Type>, VersionRange>(type, version));
        //}

        private static readonly List<Tuple<Func<IUpgradeScript>, VersionRange>> Scripts = new List<Tuple<Func<IUpgradeScript>, VersionRange>>();
        //private static readonly List<Tuple<Lazy<Type>, VersionRange>> Scripts = new List<Tuple<Lazy<Type>, VersionRange>>();
    }
}