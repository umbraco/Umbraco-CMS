using System;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_0_0;

public class AddMemberPropertiesAsColumns : MigrationBase
{
    public AddMemberPropertiesAsColumns(IMigrationContext context) : base(context)
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
            .Select("MAX([cv].[id]) as [id]", "[cv].[nodeId]")
            .From<ContentVersionDto>("cv")
            .GroupBy("[cv].[nodeId]");

        Sql<ISqlContext> passwordAttemptsQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberFailedPasswordAttempts'");

        Sql<ISqlContext> memberApprovedQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberApproved'");

        Sql<ISqlContext> memberLockedOutQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberLockedOut'");

        Sql<ISqlContext> memberLastLockoutDateQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberLastLockoutDate'");

        Sql<ISqlContext> memberLastLoginDateQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberLastLogin'");

        Sql<ISqlContext> memberLastPasswordChangeDateQuery = Database.SqlContext.Sql()
            .Select("[contentTypeId]", "[dataTypeId]", "[id]")
            .From<PropertyTypeDto>("pt")
            .Where("[pt].[Alias] = 'umbracoMemberLastPasswordChangeDate'");


        Sql<ISqlContext> columnDataQuery = Database.SqlContext.Sql()
            .Select(
                "[cmsMember].[nodeId]",
                "[umbracoPropertyData].[intValue] as [failedPasswordAttempts]",
                "[pdmp].[intValue] as [memberApproved]",
                "[pdlo].[intValue] as [isLockedOut]",
                "[pdlout].[dateValue] as [lastLockoutDate]",
                "[pdlin].[dateValue] as [lastLoginDate]",
                "[pdlpc].[dateValue] as [lastPasswordChangeDate]"
            )
            .From<NodeDto>()
            .InnerJoin<ContentDto>()
            .On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId)
            .InnerJoin(newestContentVersionQuery, "umbracoContentVersion")
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<MemberDto>()
            .On<ContentDto, MemberDto>((left, right) => left.NodeId == right.NodeId)
            .LeftJoin(passwordAttemptsQuery, "failedAttemptsType")
            .On<ContentDto, FailedAttempts>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>()
            .On<FailedAttempts, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
            .LeftJoin<PropertyDataDto>()
            .On<PropertyDataDto, FailedAttempts, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id)
            .LeftJoin(memberApprovedQuery, "memberApprovedType")
            .On<ContentDto, MemberApproved>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtmp")
            .On<MemberApproved, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtmp")
            .LeftJoin<PropertyDataDto>("pdmp")
            .On<PropertyDataDto, MemberApproved, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdmp")
            .LeftJoin(memberLockedOutQuery, "memberLockedOutType")
            .On<ContentDto, MemberLockedOut>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlo")
            .On<MemberLockedOut, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlo")
            .LeftJoin<PropertyDataDto>("pdlo")
            .On<PropertyDataDto, MemberLockedOut, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlo")
            .LeftJoin(memberLastLockoutDateQuery, "lastLockOutDateType")
            .On<ContentDto, LastLockoutDate>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlout")
            .On<LastLockoutDate, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlout")
            .LeftJoin<PropertyDataDto>("pdlout")
            .On<PropertyDataDto, LastLockoutDate, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlout")
            .LeftJoin(memberLastLoginDateQuery, "lastLoginDateType")
            .On<ContentDto, LastLoginDate>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlin")
            .On<LastLoginDate, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlin")
            .LeftJoin<PropertyDataDto>("pdlin")
            .On<PropertyDataDto, LastLoginDate, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlin")
            .LeftJoin(memberLastPasswordChangeDateQuery, "lastPasswordChangeType")
            .On<ContentDto, LastPasswordChange>((left, right) => left.ContentTypeId == right.ContentTypeId)
            .LeftJoin<DataTypeDto>("dtlpc")
            .On<LastPasswordChange, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId, null, "dtlpc")
            .LeftJoin<PropertyDataDto>("pdlpc")
            .On<PropertyDataDto, LastPasswordChange, ContentVersionDto>((left, middle, right) => left.PropertyTypeId == middle.Id && left.VersionId == right.Id, "pdlpc")
            .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Member);

        var columnData = Database.Fetch<MemberColumnData>(columnDataQuery);
    }

    private class MemberColumnData
    {
        [Column("nodeId")]
        public int Id { get; set; }

        [Column("failedPasswordAttempts")]
        public int? FailedPasswordAttempts { get; set; }

        [Column("memberApproved")]
        public bool MemberApproved { get; set; }

        [Column("isLockedOut")]
        public bool? IsLockedOut { get; set; }

        [Column("lastLockoutDate")]
        public DateTime? LastLockoutDate { get; set; }

        [Column("lastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [Column("lastPasswordChangeDate")]
        public DateTime? LastPasswordChangeDate { get; set; }
    }

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
