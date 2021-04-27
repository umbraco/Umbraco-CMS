using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Migrations.Events
{
    internal class DatabaseSchemaCreatingNotification : CancelableNotification
    {
        public DatabaseSchemaCreatingNotification(EventMessages messages) : base(messages)
        {
        }
    }
}
