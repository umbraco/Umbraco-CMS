using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class UserDto
    {
        public UserDto()
        {
            UserGroupDtos = new List<UserGroupDto>();
            UserStartNodeDtos = new HashSet<UserStartNodeDto>();
        }

        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_user")]
        public int Id { get; set; }

        [Column("userDisabled")]
        [Constraint(Default = "0")]
        public bool Disabled { get; set; }

        [Column("userNoConsole")]
        [Constraint(Default = "0")]
        public bool NoConsole { get; set; }
        
        [Column("userName")]
        public string UserName { get; set; }

        [Column("userLogin")]
        [Length(125)]
        [Index(IndexTypes.NonClustered)]
        public string Login { get; set; }

        [Column("userPassword")]
        [Length(500)]
        public string Password { get; set; }

        /// <summary>
        /// This will represent a JSON structure of how the password has been created (i.e hash algorithm, iterations)
        /// </summary>
        [Column("passwordConfig")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string PasswordConfig { get; set; }

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

        [Column("emailConfirmedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? EmailConfirmedDate { get; set; }

        [Column("invitedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? InvitedDate { get; set; }

        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }

        [Column("updateDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Will hold the media file system relative path of the users custom avatar if they uploaded one
        /// </summary>
        [Column("avatar")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string Avatar { get; set; }

        [ResultColumn]
        public List<UserGroupDto> UserGroupDtos { get; set; }

        [ResultColumn]
        public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
    }
}