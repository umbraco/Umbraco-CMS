using System.Text;
using Umbraco.Core.Composing;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.ModelsBuilder.Umbraco;

namespace Umbraco.ModelsBuilder.Dashboard
{
    internal static class BuilderDashboardHelper
    {
        private static Config Config => Current.Configs.ModelsBuilder();

        public static bool CanGenerate()
        {
            return Config.ModelsMode.SupportsExplicitGeneration();
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
            var config = Config;

            if (!config.Enable)
                return "Version: " + Api.ApiVersion.Current.Version + "<br />&nbsp;<br />ModelsBuilder is disabled<br />(the .Enable key is missing, or its value is not 'true').";

            var sb = new StringBuilder();

            sb.Append("Version: ");
            sb.Append(Api.ApiVersion.Current.Version);
            sb.Append("<br />&nbsp;<br />");

            sb.Append("ModelsBuilder is enabled, with the following configuration:");

            sb.Append("<ul>");

            sb.Append("<li>The <strong>models factory</strong> is ");
            sb.Append(config.EnableFactory || config.ModelsMode == ModelsMode.PureLive
                ? "enabled"
                : "not enabled. Umbraco will <em>not</em> use models");
            sb.Append(".</li>");

            sb.Append(config.ModelsMode != ModelsMode.Nothing
                ? $"<li><strong>{config.ModelsMode} models</strong> are enabled.</li>"
                : "<li>No models mode is specified: models will <em>not</em> be generated.</li>");

            sb.Append($"<li>Models namespace is {config.ModelsNamespace}.</li>");

            sb.Append("<li>Tracking of <strong>out-of-date models</strong> is ");
            sb.Append(config.FlagOutOfDateModels ? "enabled" : "not enabled");
            sb.Append(".</li>");

            sb.Append("</ul>");

            return sb.ToString();
        }
    }
}
