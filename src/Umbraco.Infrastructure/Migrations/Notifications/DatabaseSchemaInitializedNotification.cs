using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications
{
    public sealed class DatabaseSchemaInitializedNotification : StatefulNotification
    {
        public DatabaseSchemaInitializedNotification(IUmbracoDatabase database)
        {
            Database = database;
        }

        public IUmbracoDatabase Database { get; }
    }
}
