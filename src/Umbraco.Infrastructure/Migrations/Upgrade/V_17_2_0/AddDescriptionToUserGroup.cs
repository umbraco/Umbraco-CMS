using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_2_0
{
    /// <summary>
    /// Migration to add a description column to the user group table.
    /// </summary>
    public class AddDescriptionToUserGroup : AsyncMigrationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddDescriptionToUserGroup"/> class.
        /// </summary>
        /// <param name="context">The migration context.</param>
        public AddDescriptionToUserGroup(
            IMigrationContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        protected override async Task MigrateAsync()
        {
            if (TableExists(Constants.DatabaseSchema.Tables.UserGroup) is false)
            {
                return;
            }

            const string ColumnName = "description";
            var hasColumn = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database)
                .Any(c =>
                    c.TableName == Constants.DatabaseSchema.Tables.UserGroup &&
                    c.ColumnName == ColumnName);

            if (hasColumn)
            {
                return;
            }

            AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, ColumnName);
        }
    }
}
