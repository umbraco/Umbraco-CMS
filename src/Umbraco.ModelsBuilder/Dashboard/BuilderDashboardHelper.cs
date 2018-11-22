using System;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.ModelsBuilder.Umbraco;

namespace Umbraco.ModelsBuilder.Dashboard
{
    internal static class BuilderDashboardHelper
    {
        public static bool CanGenerate()
        {
            return UmbracoConfig.For.ModelsBuilder().ModelsMode.SupportsExplicitGeneration();
        }

        public static bool GenerateCausesRestart()
        {
            return UmbracoConfig.For.ModelsBuilder().ModelsMode.IsAnyDll();
        }

        public static bool AreModelsOutOfDate()
        {
            return OutOfDateModelsStatus.IsOutOfDate;
        }

        public static string LastError()
        {
            return ModelsGenerationError.GetLastError();
        }

        public static string Text()
        {
            var config = UmbracoConfig.For.ModelsBuilder();
            if (!config.Enable)
                return "ModelsBuilder is disabled<br />(the .Enable key is missing, or its value is not 'true').";

            var sb = new StringBuilder();

            sb.Append("ModelsBuilder is enabled, with the following configuration:");

            sb.Append("<ul>");

            sb.Append("<li>The <strong>models factory</strong> is ");
            sb.Append(config.EnableFactory || config.ModelsMode == ModelsMode.PureLive
                ? "enabled"
                : "not enabled. Umbraco will <em>not</em> use models");
            sb.Append(".</li>");

            sb.Append("<li>The <strong>API</strong> is ");
            if (config.ApiInstalled && config.EnableApi)
            {
                sb.Append("installed and enabled");
                if (!config.IsDebug) sb.Append(".<br />However, the API runs only with <em>debug</em> compilation mode");
            }
            else if (config.ApiInstalled || config.EnableApi)
                sb.Append(config.ApiInstalled ? "installed but not enabled" : "enabled but not installed");
            else sb.Append("neither installed nor enabled");
            sb.Append(".<br />");
            if (!config.ApiServer)
                sb.Append("External tools such as Visual Studio <em>cannot</em> use the API");
            else
                sb.Append("<span style=\"color:orange;font-weight:bold;\">The API endpoint is open on this server</span>");
            sb.Append(".</li>");

            sb.Append(config.ModelsMode != ModelsMode.Nothing
                ? $"<li><strong>{config.ModelsMode} models</strong> are enabled.</li>"
                : "<li>No models mode is specified: models will <em>not</em> be generated.</li>");

            sb.Append($"<li>Models namespace is {config.ModelsNamespace}.</li>");

            sb.Append("<li>Static mixin getters are ");
            sb.Append(config.StaticMixinGetters ? "enabled" : "disabled");
            if (config.StaticMixinGetters)
            {
                sb.Append(". The pattern for getters is ");
                sb.Append(string.IsNullOrWhiteSpace(config.StaticMixinGetterPattern)
                    ? "not configured (will use default)"
                    : $"\"{config.StaticMixinGetterPattern}\"");
            }
            sb.Append(".</li>");

            sb.Append("<li>Tracking of <strong>out-of-date models</strong> is ");
            sb.Append(config.FlagOutOfDateModels ? "enabled" : "not enabled");
            sb.Append(".</li>");

            sb.Append("</ul>");

            return sb.ToString();
        }
    }
}
