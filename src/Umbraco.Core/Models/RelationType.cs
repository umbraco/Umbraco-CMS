using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a RelationType
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RelationType : Entity, IAggregateRoot, IRelationType
    {
        private string _name;
        private string _alias;
        private bool _isBidrectional;
        private Guid _parentObjectType;
        private Guid _childObjectType;

        public RelationType(Guid childObjectType, Guid parentObjectType, string @alias)
        {
            Mandate.ParameterNotNullOrEmpty(@alias, "alias");
            _childObjectType = childObjectType;
            _parentObjectType = parentObjectType;
            _alias = alias;
            Name = _alias;
        }

        public RelationType(Guid childObjectType, Guid parentObjectType, string @alias, string name)
            :this(childObjectType, parentObjectType, @alias)
        {
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Name = name;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<RelationType, string>(x => x.Name);
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<RelationType, string>(x => x.Alias);
            public readonly PropertyInfo IsBidirectionalSelector = ExpressionHelper.GetPropertyInfo<RelationType, bool>(x => x.IsBidirectional);
            public readonly PropertyInfo ParentObjectTypeSelector = ExpressionHelper.GetPropertyInfo<RelationType, Guid>(x => x.ParentObjectType);
            public readonly PropertyInfo ChildObjectTypeSelector = ExpressionHelper.GetPropertyInfo<RelationType, Guid>(x => x.ChildObjectType);
        }

        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set { SetPropertyValueAndDetectChanges(value, ref _alias, Ps.Value.AliasSelector); }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember]
        public bool IsBidirectional
        {
            get { return _isBidrectional; }
            set { SetPropertyValueAndDetectChanges(value, ref _isBidrectional, Ps.Value.IsBidirectionalSelector); }
        }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ParentObjectType
        {
            get { return _parentObjectType; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentObjectType, Ps.Value.ParentObjectTypeSelector); }
        }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ChildObjectType
        {
            get { return _childObjectType; }
            set { SetPropertyValueAndDetectChanges(value, ref _childObjectType, Ps.Value.ChildObjectTypeSelector); }
        }

    }
}