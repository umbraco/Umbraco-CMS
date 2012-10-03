using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UserDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("userDisabled")]
        public bool Disabled { get; set; }

        [Column("userNoConsole")]
        public bool NoConsole { get; set; }

        [Column("userType")]
        public short Type { get; set; }

        [Column("startStructureID")]
        public int ContentStartId { get; set; }

        [Column("startMediaID")]
        public int? MediaStartId { get; set; }

        [Column("userName")]
        public string UserName { get; set; }

        [Column("userLogin")]
        public string Login { get; set; }

        [Column("userPassword")]
        public string Password { get; set; }

        [Column("userEmail")]
        public string Email { get; set; }

        [Column("userDefaultPermissions")]
        public string DefaultPermissions { get; set; }

        [Column("userLanguage")]
        public string UserLanguage { get; set; }

        [Column("defaultToLiveEditing")]
        public bool DefaultToLiveEditing { get; set; }
    }
}