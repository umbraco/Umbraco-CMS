using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

internal class DashboardReport
{
    private readonly ModelsBuilderSettings _config;
    private readonly ModelsGenerationError _mbErrors;
    private readonly OutOfDateModelsStatus _outOfDateModels;

    public DashboardReport(IOptions<ModelsBuilderSettings> config, OutOfDateModelsStatus outOfDateModels,
        ModelsGenerationError mbErrors)
    {
        _config = config.Value;
        _outOfDateModels = outOfDateModels;
        _mbErrors = mbErrors;
    }

    public bool CanGenerate() => _config.ModelsMode.SupportsExplicitGeneration();

    public bool AreModelsOutOfDate() => _outOfDateModels.IsOutOfDate;

    public string? LastError() => _mbErrors.GetLastError();

    public string Text()
    {
        var sb = new StringBuilder();

        sb.Append("<p>Version: ");
        sb.Append(ApiVersion.Current.Version);
        sb.Append("</p>");

        sb.Append("<p>ModelsBuilder is enabled, with the following configuration:</p>");

        sb.Append("<ul>");

        sb.Append("<li>The <strong>models mode</strong> is '");
        sb.Append(_config.ModelsMode.ToString());
        sb.Append("'. ");

        switch (_config.ModelsMode)
        {
            case ModelsMode.Nothing:
                sb.Append(
                    "Strongly typed models are not generated. All content and cache will operate from instance of IPublishedContent only.");
                break;
            case ModelsMode.InMemoryAuto:
                sb.Append(
                    "Strongly typed models are re-generated on startup and anytime schema changes (i.e. Content Type) are made. No recompilation necessary but the generated models are not available to code outside of Razor.");
                break;
            case ModelsMode.SourceCodeManual:
                sb.Append(
                    "Strongly typed models are generated on demand. Recompilation is necessary and models are available to all CSharp code.");
                break;
            case ModelsMode.SourceCodeAuto:
                sb.Append(
                    "Strong typed models are generated on demand and anytime schema changes (i.e. Content Type) are made. Recompilation is necessary and models are available to all CSharp code.");
                break;
        }

        sb.Append("</li>");

        if (_config.ModelsMode != ModelsMode.Nothing)
        {
            sb.Append(
                $"<li>Models namespace is {_config.ModelsNamespace ?? Constants.ModelsBuilder.DefaultModelsNamespace}.</li>");

            sb.Append("<li>Tracking of <strong>out-of-date models</strong> is ");
            sb.Append(_config.FlagOutOfDateModels ? "enabled" : "not enabled");
            sb.Append(".</li>");
        }

        sb.Append("</ul>");

        return sb.ToString();
    }
}
