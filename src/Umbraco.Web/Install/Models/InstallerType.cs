namespace Umbraco.Web.Install.Models
{
    public enum InstallerType
    {
        /// <summary>
        /// Represents a normal Core Umbraco intallation procedure
        /// </summary>
        Core,

        /// <summary>
        /// Represents a package migrations installer procedure
        /// </summary>
        Migrations
    }
}