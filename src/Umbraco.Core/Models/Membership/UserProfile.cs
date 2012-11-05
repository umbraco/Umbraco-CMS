using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Profile implementation for a backoffice User
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    internal class UserProfile : Profile
    {
        public UserProfile()
        {
            SessionTimeout = 60;
            Applications = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        /// <value>
        /// The session timeout.
        /// </value>
        public int SessionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the start content id.
        /// </summary>
        /// <value>
        /// The start content id.
        /// </value>
        public int StartContentId { get; set; }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>
        /// The start media id.
        /// </value>
        public int StartMediaId { get; set; }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public IEnumerable<string> Applications { get; set; }
    }
}