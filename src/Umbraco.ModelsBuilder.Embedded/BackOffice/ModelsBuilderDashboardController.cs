using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;

namespace Umbraco.ModelsBuilder.Embedded.BackOffice
{
    /// <summary>
    /// API controller for use in the Umbraco back office with Angular resources
    /// </summary>
    /// <remarks>
    /// We've created a different controller for the backoffice/angular specifically this is to ensure that the
    /// correct CSRF security is adhered to for angular and it also ensures that this controller is not subseptipal to
    /// global WebApi formatters being changed since this is always forced to only return Angular JSON Specific formats.
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    public class ModelsBuilderDashboardController : UmbracoAuthorizedJsonController
    {
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly ModelsGenerationError _mbErrors;
        private readonly DashboardReport _dashboardReport;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ModelsBuilderDashboardController(IOptions<ModelsBuilderSettings> config, ModelsGenerator modelsGenerator, OutOfDateModelsStatus outOfDateModels, ModelsGenerationError mbErrors, IHostingEnvironment hostingEnvironment)
        {
            //_umbracoServices = umbracoServices;
            _config = config.Value;
            _modelGenerator = modelsGenerator;
            _outOfDateModels = outOfDateModels;
            _mbErrors = mbErrors;
            _dashboardReport = new DashboardReport(config, outOfDateModels, mbErrors);
            _hostingEnvironment = hostingEnvironment;
        }

        // invoked by the dashboard
        // requires that the user is logged into the backoffice and has access to the settings section
        // beware! the name of the method appears in modelsbuilder.controller.js
        [HttpPost] // use the http one, not mvc, with api controllers!
        public IActionResult BuildModels()
        {
            try
            {
                var config = _config;

                if (!config.ModelsMode.SupportsExplicitGeneration())
                {
                    var result2 = new BuildResult { Success = false, Message = "Models generation is not enabled." };
                    return Ok(result2);
                }

                var bin = _hostingEnvironment.MapPathContentRoot("~/bin");
                if (bin == null)
                    throw new PanicException("bin is null.");

                // EnableDllModels will recycle the app domain - but this request will end properly
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
            var status = _outOfDateModels.IsEnabled
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
        public ActionResult<Dashboard> GetDashboard()
        {
            return GetDashboardResult();
        }

        private Dashboard GetDashboardResult()
        {
            return new Dashboard
            {
                Enable = _config.Enable,
                Text = _dashboardReport.Text(),
                CanGenerate = _dashboardReport.CanGenerate(),
                OutOfDateModels = _dashboardReport.AreModelsOutOfDate(),
                LastError = _dashboardReport.LastError(),
            };
        }

        [DataContract]
        public class BuildResult
        {
            [DataMember(Name = "success")]
            public bool Success;
            [DataMember(Name = "message")]
            public string Message;
        }

        [DataContract]
        public class Dashboard
        {
            [DataMember(Name = "enable")]
            public bool Enable;
            [DataMember(Name = "text")]
            public string Text;
            [DataMember(Name = "canGenerate")]
            public bool CanGenerate;
            [DataMember(Name = "outOfDateModels")]
            public bool OutOfDateModels;
            [DataMember(Name = "lastError")]
            public string LastError;
        }

        public enum OutOfDateType
        {
            OutOfDate,
            Current,
            Unknown = 100
        }

        [DataContract]
        public class OutOfDateStatus
        {
            [DataMember(Name = "status")]
            public OutOfDateType Status { get; set; }
        }
    }
}
