using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// A <see cref="HashSet{T}"/> of <see cref="EntityPermission"/>
    /// </summary>
    public class EntityPermissionCollection : HashSet<EntityPermission>
    {
        public EntityPermissionCollection()
        {
        }

        public EntityPermissionCollection(IEnumerable<EntityPermission> collection) : base(collection)
        {
        }

        /// <summary>
        /// Returns the aggregate permissions in the permission set
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This value is only calculated once
        /// </remarks>
        public IEnumerable<string> GetAllPermissions()
        {
            return _aggregatePermissions ?? (_aggregatePermissions =
                       this.SelectMany(x => x.AssignedPermissions).Distinct().ToArray());
        }

        private string[] _aggregatePermissions;
    }
}