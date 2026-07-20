using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(MemberPropertyTypeDtoConfiguration))]
public sealed class MemberPropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.MemberPropertyType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string PropertyTypeIdColumnName = "propertytypeId";

    public int PrimaryKey { get; set; }

    public int NodeId { get; set; }

    public int PropertyTypeId { get; set; }

    public bool CanEdit { get; set; }

    public bool ViewOnProfile { get; set; }

    public bool IsSensitive { get; set; }

    public PropertyTypeDto PropertyTypeDto { get; set; } = null!;
}
