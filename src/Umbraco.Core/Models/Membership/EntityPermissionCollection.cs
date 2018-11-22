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
        /// Returns the aggregate permissions in the permission set for a single node
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This value is only calculated once per node
        /// </remarks>
        public IEnumerable<string> GetAllPermissions(int entityId)
        {
            if (_aggregateNodePermissions == null)
                _aggregateNodePermissions = new Dictionary<int, string[]>();

            string[] entityPermissions;
            if (_aggregateNodePermissions.TryGetValue(entityId, out entityPermissions) == false)
            {
                entityPermissions = this.Where(x => x.EntityId == entityId).SelectMany(x => x.AssignedPermissions).Distinct().ToArray();
                _aggregateNodePermissions[entityId] = entityPermissions;
            }
            return entityPermissions;
        }

        private Dictionary<int, string[]> _aggregateNodePermissions;

        /// <summary>
        /// Returns the aggregate permissions in the permission set for all nodes
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