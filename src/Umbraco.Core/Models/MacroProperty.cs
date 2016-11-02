using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro Property
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MacroProperty : TracksChangesEntityBase, IMacroProperty, IRememberBeingDirty, IDeepCloneable
    {
        public MacroProperty()
        {
            
        }

        /// <summary>
        /// Ctor for creating a new property
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="sortOrder"></param>
        /// <param name="editorAlias"></param>
        public MacroProperty(string @alias, string name, int sortOrder, string editorAlias)
        {
            _alias = alias;
            _name = name;
            _sortOrder = sortOrder;

            //try to get the new mapped parameter editor
            var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(editorAlias, false);
            if (mapped.IsNullOrWhiteSpace() == false)
            {
                editorAlias = mapped;
            }

            _editorAlias = editorAlias;
        }

        /// <summary>
        /// Ctor for creating an existing property
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="sortOrder"></param>
        /// <param name="editorAlias"></param>
        internal MacroProperty(int id, string @alias, string name, int sortOrder, string editorAlias)
        {
            _id = id;
            _alias = alias;
            _name = name;
            _sortOrder = sortOrder;

            //try to get the new mapped parameter editor
            var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(editorAlias, false);
            if (mapped.IsNullOrWhiteSpace() == false)
            {
                editorAlias = mapped;
            }

            _editorAlias = editorAlias;
        }

        private string _alias;
        private string _name;
        private int _sortOrder;
        private int _id;
        private string _editorAlias;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, string>(x => x.Alias);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, string>(x => x.Name);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, int>(x => x.SortOrder);
            public readonly PropertyInfo IdSelector = ExpressionHelper.GetPropertyInfo<Entity, int>(x => x.Id);
            public readonly PropertyInfo PropertyTypeSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, string>(x => x.EditorAlias);
        }

        /// <summary>
        /// Gets or sets the Alias of the Property
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return _id; }
            set { SetPropertyValueAndDetectChanges(value, ref _id, Ps.Value.IdSelector); }
        }

        /// <summary>
        /// Gets or sets the Alias of the Property
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set { SetPropertyValueAndDetectChanges(value, ref _alias, Ps.Value.AliasSelector); }
        }

        /// <summary>
        /// Gets or sets the Name of the Property
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Property
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Gets or sets the Type for this Property
        /// </summary>
        /// <remarks>
        /// The MacroPropertyTypes acts as a plugin for Macros.
        /// All types was previously contained in the database, but has been ported to code.
        /// </remarks>
        [DataMember]
        public string EditorAlias
        {
            get { return _editorAlias; }
            set
            {
                //try to get the new mapped parameter editor
                var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(value, false);
                var newVal = mapped.IsNullOrWhiteSpace() == false ? mapped : value;
                SetPropertyValueAndDetectChanges(newVal, ref _editorAlias, Ps.Value.PropertyTypeSelector);                
            }
        }

        public object DeepClone()
        {
            //Memberwise clone on MacroProperty will work since it doesn't have any deep elements
            // for any sub class this will work for standard properties as well that aren't complex object's themselves.
            var clone = (MacroProperty)MemberwiseClone();
            //Automatically deep clone ref properties that are IDeepCloneable
            DeepCloneHelper.DeepCloneRefProperties(this, clone);
            clone.ResetDirtyProperties(false);
            return clone;
        }

        protected bool Equals(MacroProperty other)
        {
            return string.Equals(_alias, other._alias) && _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MacroProperty) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_alias != null ? _alias.GetHashCode() : 0)*397) ^ _id;
            }
        }
    }
}