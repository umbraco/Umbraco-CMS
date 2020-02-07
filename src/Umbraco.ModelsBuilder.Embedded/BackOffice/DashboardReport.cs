using System.Text;
using Umbraco.ModelsBuilder.Embedded.Configuration;

namespace Umbraco.ModelsBuilder.Embedded.BackOffice
{
    internal class DashboardReport
    {
        private readonly IModelsBuilderConfig _config;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly ModelsGenerationError _mbErrors;

        public DashboardReport(IModelsBuilderConfig config, OutOfDateModelsStatus outOfDateModels, ModelsGenerationError mbErrors)
        {
            _config = config;
            _outOfDateModels = outOfDateModels;
            _mbErrors = mbErrors;
        }

        public bool CanGenerate() => _config.ModelsMode.SupportsExplicitGeneration();

        public bool AreModelsOutOfDate() => _outOfDateModels.IsOutOfDate;

        public string LastError() => _mbErrors.GetLastError();

        public string Text()
        {
            if (!_config.Enable)
                return "Version: " + ApiVersion.Current.Version + "<br />&nbsp;<br />ModelsBuilder is disabled<br />(the .Enable key is missing, or its value is not 'true').";

            var sb = new StringBuilder();

            sb.Append("Version: ");
            sb.Append(ApiVersion.Current.Version);
            sb.Append("<br />&nbsp;<br />");

            sb.Append("ModelsBuilder is enabled, with the following configuration:");

            sb.Append("<ul>");

            sb.Append("<li>The <strong>models factory</strong> is ");
            sb.Append(_config.EnableFactory || _config.ModelsMode == ModelsMode.PureLive
                ? "enabled"
                : "not enabled. Umbraco will <em>not</em> use models");
            sb.Append(".</li>");

            sb.Append(_config.ModelsMode != ModelsMode.Nothing
                ? $"<li><strong>{_config.ModelsMode} models</strong> are enabled.</li>"
                : "<li>No models mode is specified: models will <em>not</em> be generated.</li>");

            sb.Append($"<li>Models namespace is {_config.ModelsNamespace}.</li>");

            sb.Append("<li>Tracking of <strong>out-of-date models</strong> is ");
            sb.Append(_config.FlagOutOfDateModels ? "enabled" : "not enabled");
            sb.Append(".</li>");

            sb.Append("</ul>");

            return sb.ToString();
        }
    }
}
