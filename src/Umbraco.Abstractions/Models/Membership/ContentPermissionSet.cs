using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an <see cref="IContent"/> -> user group & permission key value pair collection
    /// </summary>
    /// <remarks>
    /// This implements <see cref="IAggregateRoot"/> purely so it can be used with the repository layer which is why it's explicitly implemented.
    /// </remarks>
    public class ContentPermissionSet : EntityPermissionSet, IEntity
    {
        private readonly IContent _content;

        public ContentPermissionSet(IContent content, EntityPermissionCollection permissionsSet)
            : base(content.Id, permissionsSet)
        {
            _content = content;
        }

        public override int EntityId
        {
            get { return _content.Id; }
        }

        #region Explicit implementation of IAggregateRoot
        int IEntity.Id
        {
            get { return EntityId; }
            set { throw new NotImplementedException(); }
        }

        bool IEntity.HasIdentity
        {
            get { return EntityId > 0; }
        }

        Guid IEntity.Key { get; set; }

        DateTime IEntity.CreateDate { get; set; }

        DateTime IEntity.UpdateDate { get; set; }

        DateTime? IEntity.DeleteDate { get; set; }

        object IDeepCloneable.DeepClone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
