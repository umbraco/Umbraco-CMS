using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class UserDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_user")]
        public int Id { get; set; }

        [Column("userDisabled")]
        [Constraint(Default = "0")]
        public bool Disabled { get; set; }

        [Column("userNoConsole")]
        [Constraint(Default = "0")]
        public bool NoConsole { get; set; }

        [Column("userType")]
        [ForeignKey(typeof(UserTypeDto))]
        public short Type { get; set; }

        [Column("startStructureID")]
        public int ContentStartId { get; set; }

        [Column("startMediaID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MediaStartId { get; set; }

        [Column("userName")]
        public string UserName { get; set; }

        [Column("userLogin")]
        [Length(125)]
        [Index(IndexTypes.NonClustered)]
        public string Login { get; set; }

        [Column("userPassword")]
        [Length(500)]
        public string Password { get; set; }

        [Column("userEmail")]
        public string Email { get; set; }
        
        [Column("userLanguage")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(10)]
        public string UserLanguage { get; set; }

        [Column("securityStampToken")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string SecurityStampToken { get; set; }

        [Column("failedLoginAttempts")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? FailedLoginAttempts { get; set; }

        [Column("lastLockoutDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastLockoutDate { get; set; }

        [Column("lastPasswordChangeDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastPasswordChangeDate { get; set; }

        [Column("lastLoginDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastLoginDate { get; set; }
        
        [ResultColumn]
        public List<User2AppDto> User2AppDtos { get; set; }
    }
}