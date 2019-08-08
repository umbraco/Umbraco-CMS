using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Implementation of <see cref="IEntitySlim"/> for internal use.
    /// </summary>
    /// <remarks>
    /// <para>Although it implements <see cref="IEntitySlim"/>, this class does not 
    /// implement <see cref="IRememberBeingDirty"/> and everything this interface defines, throws.</para>
    /// <para>Although it implements <see cref="IEntitySlim"/>, this class does not
    /// implement <see cref="IDeepCloneable"/> and deep-cloning throws.</para>
    /// </remarks>
    public class EntitySlim : IEntitySlim
    {
        private IDictionary<string, object> _additionalData;

        /// <summary>
        /// Gets an entity representing "root".
        /// </summary>
        public static readonly IEntitySlim Root = new EntitySlim { Path = "-1", Name = "root", HasChildren = true };
        
        // implement IEntity

        /// <inheritdoc />
        [DataMember]
        public int Id { get; set; }

        /// <inheritdoc />
        [DataMember]
        public Guid Key { get; set; }

        /// <inheritdoc />
        [DataMember]
        public DateTime CreateDate { get; set; }

        /// <inheritdoc />
        [DataMember]
        public DateTime UpdateDate { get; set; }

        /// <inheritdoc />
        [DataMember]
        public DateTime? DeleteDate { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool HasIdentity => Id != 0;


        // implement ITreeEntity

        /// <inheritdoc />
        [DataMember]
        public string Name { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int CreatorId { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int ParentId { get; set; }

        /// <inheritdoc />
        public void SetParent(ITreeEntity parent) => throw new WontImplementException();

        /// <inheritdoc />
        [DataMember]
        public int Level { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Path { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int SortOrder { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool Trashed { get; set; }


        // implement IUmbracoEntity

        /// <inheritdoc />
        [DataMember]
        public IDictionary<string, object> AdditionalData => _additionalData ?? (_additionalData = new Dictionary<string, object>());

        /// <inheritdoc />
        [IgnoreDataMember]
        public bool HasAdditionalData => _additionalData != null;


        // implement IEntitySlim

        /// <inheritdoc />
        [DataMember]
        public Guid NodeObjectType { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool HasChildren { get; set; }

        /// <inheritdoc />
        [DataMember]
        public virtual bool IsContainer { get; set; }


        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            throw new WontImplementException();
        }

        #endregion

        #region IRememberBeingDirty

        // IEntitySlim does *not* track changes, but since it indirectly implements IUmbracoEntity,
        // and therefore IRememberBeingDirty, we have to have those methods - which all throw.

        public bool IsDirty()
        {
            throw new WontImplementException();
        }

        public bool IsPropertyDirty(string propName)
        {
            throw new WontImplementException();
        }

        public IEnumerable<string> GetDirtyProperties()
        {
            throw new WontImplementException();
        }

        public void ResetDirtyProperties()
        {
            throw new WontImplementException();
        }

        public bool WasDirty()
        {
            throw new WontImplementException();
        }

        public bool WasPropertyDirty(string propertyName)
        {
            throw new WontImplementException();
        }

        public void ResetWereDirtyProperties()
        {
            throw new WontImplementException();
        }

        public void ResetDirtyProperties(bool rememberDirty)
        {
            throw new WontImplementException();
        }

        public IEnumerable<string> GetWereDirtyProperties()
        {
            throw new WontImplementException();
        }

        #endregion
    }
}
