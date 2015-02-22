using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeRoleManager : RoleManager<BackOfficeIdentityRole>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store">The IRoleStore is responsible for commiting changes via the UpdateAsync/CreateAsync methods</param>
        public BackOfficeRoleManager(IRoleStore<BackOfficeIdentityRole> store) : base(store)
        {
        }

        public static BackOfficeRoleManager Create(
            IdentityFactoryOptions<BackOfficeRoleManager> options)
        {
            //TODO: Set this up!

            var manager = new BackOfficeRoleManager(new BackOfficeRoleStore());
            
            return manager;
        }
    }
}