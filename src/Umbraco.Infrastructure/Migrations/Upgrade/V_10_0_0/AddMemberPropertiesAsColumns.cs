using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_0_0;

public class AddMemberPropertiesAsColumns : MigrationBase
{
    public AddMemberPropertiesAsColumns(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<MemberDto>(columns, "failedPasswordAttempts");
        AddColumnIfNotExists<MemberDto>(columns, "isLockedOut");
        AddColumnIfNotExists<MemberDto>(columns, "isApproved");
        AddColumnIfNotExists<MemberDto>(columns, "lastLoginDate");
        AddColumnIfNotExists<MemberDto>(columns, "lastLockoutDate");
        AddColumnIfNotExists<MemberDto>(columns, "lastPasswordChangeDate");

        Sql<ISqlContext> newestContentVersionQuery = Database.SqlContext.Sql()
            .Select(
                $"MAX({GetQuotedSelector("cv", "id")}) as {SqlSyntax.GetQuotedColumnName("id")}",
                GetQuotedSelector("cv", "nodeId"))
            .From<ContentVersionDto>("cv")
            .GroupBy(GetQuotedSelector("cv", "nodeId"));

        Sql<ISqlContext> passwordAttemptsQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberFailedPasswordAttempts'");

        Sql<ISqlContext> memberApprovedQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberApproved'");

        Sql<ISqlContext> memberLockedOutQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberLockedOut'");

        Sql<ISqlContext> memberLastLockoutDateQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberLastLockoutDate'");

        Sql<ISqlContext> memberLastLoginDateQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberLastLogin'");

        Sql<ISqlContext> memberLastPasswordChangeDateQuery = Database.SqlContext.Sql()
            .Select(GetSubQueryColumns())
            .From<PropertyTypeDto>("pt")
            .Where($"{GetQuotedSelector("pt", "Alias")} = 'umbracoMemberLastPasswordChangeDate'");

        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine($"UPDATE {Constants.DatabaseSchema.Tables.Member}");
        queryBuilder.AppendLine("SET");
        queryBuilder.AppendLine(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.FailedPasswordAttempts)} = {GetQuotedSelector("umbracoPropertyData", "intValue")},");
        queryBuilder.AppendLine(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.IsApproved)} = {GetQuotedSelector("pdmp", "intValue")},");
        queryBuilder.AppendLine(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.IsLockedOut)} = {GetQuotedSelector("pdlo", "intValue")},");
        queryBuilder.AppendLine(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.LastLockoutDate)} = {GetQuotedSelector("pdlout", "dateValue")},");
        queryBuilder.AppendLine(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.LastLoginDate)} = {GetQuotedSelector("pdlin", "dateValue")},");
        queryBuilder.Append(
            $"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<MemberDto>(x => x.LastPasswordChangeDate)} = {GetQuotedSelector("pdlpc", "dateValue")}");

        Sql<ISqlContext> updateMemberColumnsQuery = Database.SqlContext.Sql(queryBuilder.ToString())
            .From<NodeDto>()
            .InnerJoin<ContentDto>()
            .On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId)
            .InnerJoin(newestContentVersionQuery, "umbracoContentVersion")
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<MemberDto>("m")
            .On<ContentDto, MemberDto>((left, right) => left.NodeId == right.NodeId, null, "m")
            .LeftJoin(passwordAttemptsQuery, "failedAttemptsType")
            .On<ContentDto, FailedAttempts>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>()
            .On<FailedAttempts, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
            .LeftJoin<PropertyDataDto>()
            .On<PropertyDataDto, FailedAttempts, ContentVersionDto>((left, middle, right) =>
                left.PropertyTypeId == middle.Id && left.VersionId == right.Id)
            .LeftJoin(memberApprovedQuery, "memberApprovedType")
            .On<ContentDto, MemberApproved>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtmp")
            .On<MemberApproved, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtmp")
            .LeftJoin<PropertyDataDto>("pdmp")
            .On<PropertyDataDto, MemberApproved, ContentVersionDto>(
                (left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdmp")
            .LeftJoin(memberLockedOutQuery, "memberLockedOutType")
            .On<ContentDto, MemberLockedOut>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlo")
            .On<MemberLockedOut, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlo")
            .LeftJoin<PropertyDataDto>("pdlo")
            .On<PropertyDataDto, MemberLockedOut, ContentVersionDto>(
                (left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlo")
            .LeftJoin(memberLastLockoutDateQuery, "lastLockOutDateType")
            .On<ContentDto, LastLockoutDate>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlout")
            .On<LastLockoutDate, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlout")
            .LeftJoin<PropertyDataDto>("pdlout")
            .On<PropertyDataDto, LastLockoutDate, ContentVersionDto>(
                (left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlout")
            .LeftJoin(memberLastLoginDateQuery, "lastLoginDateType")
            .On<ContentDto, LastLoginDate>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlin")
            .On<LastLoginDate, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlin")
            .LeftJoin<PropertyDataDto>("pdlin")
            .On<PropertyDataDto, LastLoginDate, ContentVersionDto>(
                (left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlin")
            .LeftJoin(memberLastPasswordChangeDateQuery, "lastPasswordChangeType")
            .On<ContentDto, LastPasswordChange>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlpc")
            .On<LastPasswordChange, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlpc")
            .LeftJoin<PropertyDataDto>("pdlpc")
            .On<PropertyDataDto, LastPasswordChange, ContentVersionDto>(
                (left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlpc")
            .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Member);

        Database.Execute(updateMemberColumnsQuery);

        // Removing old property types and values, since these are no longer needed
        // Hard coding the aliases, since we want to be able to delete the constants...
        string[] propertyTypesToDelete =
        {
            "umbracoMemberFailedPasswordAttempts",
            "umbracoMemberApproved",
            "umbracoMemberLockedOut",
            "umbracoMemberLastLockoutDate",
            "umbracoMemberLastLogin",
            "umbracoMemberLastPasswordChangeDate",
        };

        Sql<ISqlContext> idQuery = Database.SqlContext.Sql().Select<PropertyTypeDto>(x => x.Id)
            .From<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(x => x.Alias, propertyTypesToDelete);
        List<int> idsToDelete = Database.Fetch<int>(idQuery);

        // Firstly deleting the property data due to FK constraints
        Sql<ISqlContext> propertyDataDelete = Database.SqlContext.Sql()
            .Delete<PropertyDataDto>()
            .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, idsToDelete);
        Database.Execute(propertyDataDelete);

        // Then we can remove the property
        Sql<ISqlContext> propertyTypeDelete = Database.SqlContext.Sql()
            .Delete<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(x => x.Id, idsToDelete);
        Database.Execute(propertyTypeDelete);
    }

    private string GetQuotedSelector(string tableName, string columnName)
        => $"{SqlSyntax.GetQuotedTableName(tableName)}.{SqlSyntax.GetQuotedColumnName(columnName)}";

    private object[] GetSubQueryColumns() => new object[]
    {
        SqlSyntax.GetQuotedColumnName("contentTypeId"), SqlSyntax.GetQuotedColumnName("dataTypeId"),
        SqlSyntax.GetQuotedColumnName("id"),
    };

    [TableName("failedAttemptsType")]
    private class FailedAttempts
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }

    [TableName("memberApprovedType")]
    private class MemberApproved
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }

    [TableName("memberLockedOutType")]
    private class MemberLockedOut
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }

    [TableName("lastLockOutDateType")]
    private class LastLockoutDate
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }

    [TableName("lastLoginDateType")]
    private class LastLoginDate
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }

    [TableName("lastPasswordChangeType")]
    private class LastPasswordChange
    {
        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("id")]
        public int Id { get; set; }
    }
}
