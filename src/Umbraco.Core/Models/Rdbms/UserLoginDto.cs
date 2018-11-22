using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserLogin")]
    [PrimaryKey("sessionId", autoIncrement = false)]
    [ExplicitColumns]
    internal class UserLoginDto
    {
        [Column("sessionId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid SessionId { get; set; }

        [Column("userId")]
        [ForeignKey(typeof(UserDto), Name = "FK_umbracoUserLogin_umbracoUser_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Tracks when the session is created
        /// </summary>
        [Column("loggedInUtc")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LoggedInUtc { get; set; }

        /// <summary>
        /// Updated every time a user's session is validated
        /// </summary>
        /// <remarks>
        /// This allows us to guess if a session is timed out if a user doesn't actively log out
        /// and also allows us to trim the data in the table
        /// </remarks>
        [Column("lastValidatedUtc")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LastValidatedUtc { get; set; }

        /// <summary>
        /// Tracks when the session is removed when the user's account is logged out
        /// </summary>
        [Column("loggedOutUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LoggedOutUtc { get; set; }

        /// <summary>
        /// Logs the IP address of the session if available
        /// </summary>
        [Column("ipAddress")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string IpAddress { get; set; }
    }
}