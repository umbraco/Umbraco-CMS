// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Infrastructure.Migrations.Test
{
    public class RunTestPlan : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueService _keyValueService;

        public RunTestPlan(
            IRuntimeState runtimeState,
            IMigrationPlanExecutor migrationPlanExecutor,
            IScopeProvider scopeProvider,
            IKeyValueService keyValueService)
        {
            _runtimeState = runtimeState;
            _migrationPlanExecutor = migrationPlanExecutor;
            _scopeProvider = scopeProvider;
            _keyValueService = keyValueService;
        }

        public void Handle(UmbracoApplicationStartingNotification notification)
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return; // Forms needs to be in run state to update
            }

            var upgrader = new Upgrader(new TestPlan());
            try
            {
                upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
