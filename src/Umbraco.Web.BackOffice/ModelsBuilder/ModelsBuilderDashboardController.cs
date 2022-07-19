using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

/// <summary>
///     API controller for use in the Umbraco back office with Angular resources
/// </summary>
/// <remarks>
///     We've created a different controller for the backoffice/angular specifically this is to ensure that the
///     correct CSRF security is adhered to for angular and it also ensures that this controller is not subseptipal to
///     global WebApi formatters being changed since this is always forced to only return Angular JSON Specific formats.
/// </remarks>
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class ModelsBuilderDashboardController : UmbracoAuthorizedJsonController
{
    public enum OutOfDateType
    {
        OutOfDate,
        Current,
        Unknown = 100
    }

    private readonly ModelsBuilderSettings _config;
    private readonly DashboardReport _dashboardReport;
    private readonly ModelsGenerationError _mbErrors;
    private readonly ModelsGenerator _modelGenerator;
    private readonly OutOfDateModelsStatus _outOfDateModels;

    public ModelsBuilderDashboardController(IOptions<ModelsBuilderSettings> config, ModelsGenerator modelsGenerator,
        OutOfDateModelsStatus outOfDateModels, ModelsGenerationError mbErrors)
    {
        _config = config.Value;
        _modelGenerator = modelsGenerator;
        _outOfDateModels = outOfDateModels;
        _mbErrors = mbErrors;
        _dashboardReport = new DashboardReport(config, outOfDateModels, mbErrors);
    }

    // invoked by the dashboard
    // requires that the user is logged into the backoffice and has access to the settings section
    // beware! the name of the method appears in modelsbuilder.controller.js
    [HttpPost] // use the http one, not mvc, with api controllers!
    public IActionResult BuildModels()
    {
        try
        {
            if (!_config.ModelsMode.SupportsExplicitGeneration())
            {
                var result2 = new BuildResult { Success = false, Message = "Models generation is not enabled." };

                return Ok(result2);
            }

            _modelGenerator.GenerateModels();
            _mbErrors.Clear();
        }
        catch (Exception e)
        {
            _mbErrors.Report("Failed to build models.", e);
        }

        return Ok(GetDashboardResult());
    }

    // invoked by the back-office
    // requires that the user is logged into the backoffice and has access to the settings section
    [HttpGet] // use the http one, not mvc, with api controllers!
    public ActionResult<OutOfDateStatus> GetModelsOutOfDateStatus()
    {
        OutOfDateStatus status = _outOfDateModels.IsEnabled
            ? _outOfDateModels.IsOutOfDate
                ? new OutOfDateStatus { Status = OutOfDateType.OutOfDate }
                : new OutOfDateStatus { Status = OutOfDateType.Current }
            : new OutOfDateStatus { Status = OutOfDateType.Unknown };

        return status;
    }

    // invoked by the back-office
    // requires that the user is logged into the backoffice and has access to the settings section
    // beware! the name of the method appears in modelsbuilder.controller.js
    [HttpGet] // use the http one, not mvc, with api controllers!
    public ActionResult<Dashboard> GetDashboard() => GetDashboardResult();

    private Dashboard GetDashboardResult() => new()
    {
        Mode = _config.ModelsMode,
        Text = _dashboardReport.Text(),
        CanGenerate = _dashboardReport.CanGenerate(),
        OutOfDateModels = _dashboardReport.AreModelsOutOfDate(),
        LastError = _dashboardReport.LastError()
    };

    [DataContract]
    public class BuildResult
    {
        [DataMember(Name = "success")] public bool Success { get; set; }

        [DataMember(Name = "message")] public string? Message { get; set; }
    }

    [DataContract]
    public class Dashboard
    {
        [DataMember(Name = "mode")] public ModelsMode Mode { get; set; }

        [DataMember(Name = "text")] public string? Text { get; set; }

        [DataMember(Name = "canGenerate")] public bool CanGenerate { get; set; }

        [DataMember(Name = "outOfDateModels")] public bool OutOfDateModels { get; set; }

        [DataMember(Name = "lastError")] public string? LastError { get; set; }
    }

    [DataContract]
    public class OutOfDateStatus
    {
        [DataMember(Name = "status")] public OutOfDateType Status { get; set; }
    }
}
