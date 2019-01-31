using System;
using System.ComponentModel;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Defines the identifiers for Umbraco system nodes.
        /// </summary>
        public static class AppSettings
        {
            // TODO: Kill me - still used in Umbraco.Core.IO.SystemFiles:27
            [Obsolete("We need to kill this appsetting as we do not use XML content cache umbraco.config anymore due to NuCache")]
            public const string ContentXML = "Umbraco.Core.ContentXML"; //umbracoContentXML


            public const string RegisterType = "Umbraco.Core.RegisterType";
            
            public const string PublishedMediaCacheSeconds = "Umbraco.Core.PublishedMediaCacheSeconds"; //"Umbraco.PublishedMediaCache.Seconds"

            public const string AssembliesAcceptingLoadExceptions = "Umbraco.Core.AssembliesAcceptingLoadExceptions"; //Umbraco.AssembliesAcceptingLoadExceptions

            public const string ConfigurationStatus = "Umbraco.Core.ConfigurationStatus"; //umbracoConfigurationStatus

            public const string Path = "Umbraco.Core.Path"; //umbracoPath

            public const string ReservedUrls = "Umbraco.Core.ReservedUrls"; //umbracoReservedUrls

            public const string ReservedPaths = "Umbraco.Core.ReservedPaths"; //umbracoReservedPaths
            
            public const string TimeOutInMinutes = "Umbraco.Core.TimeOutInMinutes"; //umbracoTimeOutInMinutes

            public const string VersionCheckPeriod = "Umbraco.Core.VersionCheckPeriod"; //umbracoVersionCheckPeriod

            public const string LocalTempStorage = "Umbraco.Core.LocalTempStorage"; //umbracoLocalTempStorage

            public const string DefaultUILanguage = "Umbraco.Core.DefaultUILanguage"; //umbracoDefaultUILanguage

            public const string HideTopLevelNodeFromPath = "Umbraco.Core.HideTopLevelNodeFromPath"; //umbracoHideTopLevelNodeFromPath

            public const string UseHttps = "Umbraco.Core.UseHttps"; //umbracoUseHttps

            public const string DisableElectionForSingleServer = "Umbraco.Core.DisableElectionForSingleServer"; //umbracoDisableElectionForSingleServer

            public const string DatabaseFactoryServerVersion = "Umbraco.Core.DatabaseFactoryServerVersion"; //Umbraco.DatabaseFactory.ServerVersion
            

            public static class Debug
            {
                public const string LogUncompletedScopes = "Umbraco.Core.LogUncompletedScopes"; //"Umbraco.CoreDebug.LogUncompletedScopes"

                public const string DumpOnTimeoutThreadAbort = "Umbraco.Core.DumpOnTimeoutThreadAbort"; //Umbraco.CoreDebug.DumpOnTimeoutThreadAbort
            }
        }
    }
}
