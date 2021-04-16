using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class MemberDto
    {
        private const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.Member;

        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentDto))]
        public int NodeId { get; set; }

        [Column("Email")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Email { get; set; }

        [Column("LoginName")]
        [Length(1000)]
        [Constraint(Default = "''")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsMember_LoginName")]
        public string LoginName { get; set; }

        [Column("Password")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Password { get; set; }

        [Column("securityStampToken")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string SecurityStampToken { get; set; }

        [Column("emailConfirmedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? EmailConfirmedDate { get; set; }

        // TODO: It would be SOOOOO much better to store all core member data here instead of hiding it in Umbraco properties

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentDto ContentDto { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}
