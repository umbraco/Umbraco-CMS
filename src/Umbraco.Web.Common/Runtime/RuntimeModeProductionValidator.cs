using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.Common.Runtime
{
    internal class RuntimeModeProductionValidator : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        private readonly ApplicationPartManager _applicationPartManager;

        public RuntimeModeProductionValidator(ApplicationPartManager applicationPartManager)
            => _applicationPartManager = applicationPartManager;

        public void Handle(UmbracoApplicationStartingNotification notification)
        {
            // Compiled Razor views are stored as application part
            if (!_applicationPartManager.ApplicationParts.Any(x => x is IRazorCompiledItemProvider))
            {
                throw new InvalidOperationException("RazorCompileOnBuild and/or RazorCompileOnPublish needs to be set to true in production mode.");
            }
        }
    }
}
