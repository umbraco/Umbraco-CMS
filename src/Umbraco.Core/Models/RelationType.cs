using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a RelationType
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RelationType : EntityBase, IRelationType
    {
        private string _name;
        private string _alias;
        private bool _isBidrectional;
        private Guid? _parentObjectType;
        private Guid? _childObjectType;

        //TODO: Should we put back the broken ctors with obsolete attributes?

        public RelationType(string alias, string name)
            : this(name, alias, false, null, null)
        {
        }

        public RelationType(string name, string alias, bool isBidrectional, Guid? parentObjectType, Guid? childObjectType)
        {
            _name = name;
            _alias = alias;
            _isBidrectional = isBidrectional;
            _parentObjectType = parentObjectType;
            _childObjectType = childObjectType;
        }



        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(value, ref _alias, nameof(Alias));
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember]
        public bool IsBidirectional
        {
            get => _isBidrectional;
            set => SetPropertyValueAndDetectChanges(value, ref _isBidrectional, nameof(IsBidirectional));
        }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid? ParentObjectType
        {
            get => _parentObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _parentObjectType, nameof(ParentObjectType));
        }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid? ChildObjectType
        {
            get => _childObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _childObjectType, nameof(ChildObjectType));
        }

    }
}
