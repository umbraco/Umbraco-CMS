using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install
{
    public class UnattendedUpgrader : INotificationAsyncHandler<RuntimeUnattendedUpgradeNotification>
    {
        private readonly IProfilingLogger _profilingLogger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IRuntimeState _runtimeState;

        public UnattendedUpgrader(IProfilingLogger profilingLogger, IUmbracoVersion umbracoVersion, DatabaseBuilder databaseBuilder, IRuntimeState runtimeState)
        {
            _profilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));
            _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
            _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        }

        public Task HandleAsync(RuntimeUnattendedUpgradeNotification notification, CancellationToken cancellationToken)
        {
            if (_runtimeState.RunUnattendedBootLogic())
            {
                // TODO: Here is also where we would run package migrations!

                var plan = new UmbracoPlan(_umbracoVersion);
                using (_profilingLogger.TraceDuration<RuntimeState>("Starting unattended upgrade.", "Unattended upgrade completed."))
                {
                    DatabaseBuilder.Result result = _databaseBuilder.UpgradeSchemaAndData(plan);
                    if (result.Success == false)
                    {
                        var innerException = new UnattendedInstallException("An error occurred while running the unattended upgrade.\n" + result.Message);
                        _runtimeState.Configure(Core.RuntimeLevel.BootFailed, Core.RuntimeLevelReason.BootFailedOnException, innerException);
                        return Task.CompletedTask;
                    }

                    notification.UnattendedUpgradeResult = RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete;
                }
            }

            return Task.CompletedTask;
        }
    }
}
