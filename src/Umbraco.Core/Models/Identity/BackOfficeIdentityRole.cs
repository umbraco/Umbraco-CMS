using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Models.Identity
{
    public class BackOfficeIdentityRole : IRole
    {
        public BackOfficeIdentityRole(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Id of the role
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the role
        /// </summary>
        public string Name { get; set; }
    }
}