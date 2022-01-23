using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "6708CA45-E96E-40B8-A40A-0607C1CA7F28",
        "Application URL Configuration",
        Description = "Checks if the Umbraco application URL is configured for your site.",
        Group = "Security")]
    public class UmbracoApplicationUrlCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtime;
        private readonly IUmbracoSettingsSection _settings;

        private const string SetApplicationUrlAction = "setApplicationUrl";

        public UmbracoApplicationUrlCheck(ILocalizedTextService textService, IRuntimeState runtime, IUmbracoSettingsSection settings)
        {
            _textService = textService;
            _runtime = runtime;
            _settings = settings;
        }

        /// <summary>
        /// Executes the action and returns its status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case SetApplicationUrlAction:
                    return SetUmbracoApplicationUrl();
                default:
                    throw new InvalidOperationException("UmbracoApplicationUrlCheck action requested is either not executable or does not exist");
            }
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckUmbracoApplicationUrl() };
        }

        private HealthCheckStatus CheckUmbracoApplicationUrl()
        {
            var url = _settings.WebRouting.UmbracoApplicationUrl;

            string resultMessage;
            StatusResultType resultType;
            var actions = new List<HealthCheckAction>();

            if (url.IsNullOrWhiteSpace())
            {
                resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultFalse");
                resultType = StatusResultType.Warning;

                actions.Add(new HealthCheckAction(SetApplicationUrlAction, Id)
                {
                    Name = _textService.Localize("healthcheck", "umbracoApplicationUrlConfigureButton"),
                    Description = _textService.Localize("healthcheck", "umbracoApplicationUrlConfigureDescription")
                });
            }
            else
            {
                resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultTrue", new[] { url });
                resultType = StatusResultType.Success;
            }

            return new HealthCheckStatus(resultMessage)
            {
                ResultType = resultType,
                Actions = actions
            };
        }

        private HealthCheckStatus SetUmbracoApplicationUrl()
        {
            var configFilePath = IOHelper.MapPath("~/config/umbracoSettings.config");
            const string xPath = "/settings/web.routing/@umbracoApplicationUrl";
            var configurationService = new ConfigurationService(configFilePath, xPath, _textService);
            var urlValue = _runtime.ApplicationUrl.ToString();
            var updateConfigFile = configurationService.UpdateConfigFile(urlValue);

            if (updateConfigFile.Success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck", "umbracoApplicationUrlConfigureSuccess", new[] { urlValue }))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
               new HealthCheckStatus(_textService.Localize("healthcheck", "umbracoApplicationUrlConfigureError", new[] { updateConfigFile.Result }))
               {
                   ResultType = StatusResultType.Error
               };
        }
    }
}
