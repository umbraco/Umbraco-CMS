using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(UserGroup2GranularPermissionDtoConfiguration))]
public class UserGroup2GranularPermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2GranularPermission;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public const string UserGroupKeyColumnName = "userGroupKey";
    public const string UniqueIdColumnName = Constants.DatabaseSchema.Columns.UniqueIdName;
    public const string PermissionColumnName = "permission";
    public const string ContextColumnName = "context";

    public int Id { get; set; }

    public Guid UserGroupKey { get; set; }

    public Guid? UniqueId { get; set; }

    public required string Permission { get; set; }

    public required string Context { get; set; }
}
