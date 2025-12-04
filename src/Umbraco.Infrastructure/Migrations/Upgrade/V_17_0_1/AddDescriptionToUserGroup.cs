using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_1
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
        /// <param name="dataTypeService">The data type service.</param>
        /// <param name="options">The TinyMce to Tiptap migration settings.</param>
        public AddDescriptionToUserGroup(
            IMigrationContext context,
            IDataTypeService dataTypeService,
            IOptions<TinyMceToTiptapMigrationSettings> options)
            : base(context)
        {
        }

        /// <inheritdoc/>
        protected override async Task MigrateAsync()
        {
            if (TableExists(Constants.DatabaseSchema.Tables.UserGroup))
            {
                var columns = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

                AddColumn(columns, "description");
            }
            return;
        }

        private void AddColumn(List<Persistence.SqlSyntax.ColumnInfo> columns, string column)
        {
            if (columns
                .SingleOrDefault(x => x.TableName == Constants.DatabaseSchema.Tables.UserGroup && x.ColumnName == column) is null)
            {
                AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, column);
            }
        }
    }
}
