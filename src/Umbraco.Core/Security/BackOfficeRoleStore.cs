using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeRoleStore : DisposableObject, IRoleStore<BackOfficeIdentityRole>
    {
        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="role"/>
        /// <returns/>
        public Task CreateAsync(BackOfficeIdentityRole role)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a role
        /// </summary>
        /// <param name="role"/>
        /// <returns/>
        public Task UpdateAsync(BackOfficeIdentityRole role)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="role"/>
        /// <returns/>
        public Task DeleteAsync(BackOfficeIdentityRole role)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a role by id
        /// </summary>
        /// <param name="roleId"/>
        /// <returns/>
        public Task<BackOfficeIdentityRole> FindByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a role by name
        /// </summary>
        /// <param name="roleName"/>
        /// <returns/>
        public Task<BackOfficeIdentityRole> FindByNameAsync(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}