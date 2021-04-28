using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Hosting;
using Umbraco.Core.Exceptions;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi.Filters;

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
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Settings)]
    public class ModelsBuilderDashboardController : UmbracoAuthorizedJsonController
    {
        private readonly IModelsBuilderConfig _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly ModelsGenerationError _mbErrors;
        private readonly DashboardReport _dashboardReport;

        public ModelsBuilderDashboardController(IModelsBuilderConfig config, ModelsGenerator modelsGenerator, OutOfDateModelsStatus outOfDateModels, ModelsGenerationError mbErrors)
        {
            //_umbracoServices = umbracoServices;
            _config = config;
            _modelGenerator = modelsGenerator;
            _outOfDateModels = outOfDateModels;
            _mbErrors = mbErrors;
            _dashboardReport = new DashboardReport(config, outOfDateModels, mbErrors);
        }

        // invoked by the dashboard
        // requires that the user is logged into the backoffice and has access to the settings section
        // beware! the name of the method appears in modelsbuilder.controller.js
        [System.Web.Http.HttpPost] // use the http one, not mvc, with api controllers!
        public HttpResponseMessage BuildModels()
        {
            try
            {
                var config = _config;

                if (!config.ModelsMode.SupportsExplicitGeneration())
                {
                    var result2 = new BuildResult { Success = false, Message = "Models generation is not enabled." };
                    return Request.CreateResponse(HttpStatusCode.OK, result2, Configuration.Formatters.JsonFormatter);
                }

                var bin = HostingEnvironment.MapPath("~/bin");
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

            return Request.CreateResponse(HttpStatusCode.OK, GetDashboardResult(), Configuration.Formatters.JsonFormatter);
        }

        // invoked by the back-office
        // requires that the user is logged into the backoffice and has access to the settings section
        [System.Web.Http.HttpGet] // use the http one, not mvc, with api controllers!
        public HttpResponseMessage GetModelsOutOfDateStatus()
        {
            var status = _outOfDateModels.IsEnabled
                ? _outOfDateModels.IsOutOfDate
                    ? new OutOfDateStatus { Status = OutOfDateType.OutOfDate }
                    : new OutOfDateStatus { Status = OutOfDateType.Current }
                : new OutOfDateStatus { Status = OutOfDateType.Unknown };

            return Request.CreateResponse(HttpStatusCode.OK, status, Configuration.Formatters.JsonFormatter);
        }

        // invoked by the back-office
        // requires that the user is logged into the backoffice and has access to the settings section
        // beware! the name of the method appears in modelsbuilder.controller.js
        [System.Web.Http.HttpGet] // use the http one, not mvc, with api controllers!
        public HttpResponseMessage GetDashboard()
        {
            return Request.CreateResponse(HttpStatusCode.OK, GetDashboardResult(), Configuration.Formatters.JsonFormatter);
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
        internal class BuildResult
        {
            [DataMember(Name = "success")]
            public bool Success;
            [DataMember(Name = "message")]
            public string Message;
        }

        [DataContract]
        internal class Dashboard
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

        internal enum OutOfDateType
        {
            OutOfDate,
            Current,
            Unknown = 100
        }

        [DataContract]
        internal class OutOfDateStatus
        {
            [DataMember(Name = "status")]
            public OutOfDateType Status { get; set; }
        }
    }
}
