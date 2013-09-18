using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro Property
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class MacroProperty : TracksChangesEntityBase, IMacroProperty, IRememberBeingDirty
    {
        public MacroProperty()
        {
            
        }

        public MacroProperty(string @alias, string name, int sortOrder, IMacroPropertyType propertyType)
        {
            _alias = alias;
            _name = name;
            _sortOrder = sortOrder;
            _propertyType = propertyType;
        }

        private string _alias;
        private string _name;
        private int _sortOrder;
        private IMacroPropertyType _propertyType;

        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, string>(x => x.Alias);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, string>(x => x.Name);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, int>(x => x.SortOrder);
        private static readonly PropertyInfo PropertyTypeSelector = ExpressionHelper.GetPropertyInfo<MacroProperty, IMacroPropertyType>(x => x.PropertyType);

        /// <summary>
        /// Gets or sets the Alias of the Property
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _alias = value;
                    return _alias;
                }, _alias, AliasSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Name of the Property
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Property
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sortOrder = value;
                    return _sortOrder;
                }, _sortOrder, SortOrderSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Type for this Property
        /// </summary>
        /// <remarks>
        /// The MacroPropertyTypes acts as a plugin for Macros.
        /// All types was previously contained in the database, but has been ported to code.
        /// </remarks>
        [DataMember]
        public IMacroPropertyType PropertyType

        {
            get { return _propertyType; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _propertyType = value;
                    return _propertyType;
                }, _propertyType, PropertyTypeSelector);
            }
        }
    }
}