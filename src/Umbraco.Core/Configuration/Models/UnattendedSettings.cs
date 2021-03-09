using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Configuration.Models
{

    /// <summary>
    /// Typed configuration options for unattended settings.
    /// </summary>
    public class UnattendedSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether unattended installs are enabled.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured and it is possible to connect to
        /// the database, but the database is empty, the runtime enters the <c>Install</c> level.
        /// If this option is set to <c>true</c> an unattended install will be performed and the runtime enters
        /// the <c>Run</c> level.</para>
        /// </remarks>
        public bool InstallUnattended { get; set; } = false;

        /// <summary>
        /// Gets or sets a value to use for creating a user with a name for Unattended Installs
        /// </summary>
        public string UnattendedUserName { get; set; } = null;

        /// <summary>
        /// Gets or sets a value to use for creating a user with an email for Unattended Installs
        /// </summary>
        [EmailAddress]
        public string UnattendedUserEmail { get; set; } = null;

        /// <summary>
        /// Gets or sets a value to use for creating a user with a password for Unattended Installs
        /// </summary>
        public string UnattendedUserPassword { get; set; } = null;
    }
}
