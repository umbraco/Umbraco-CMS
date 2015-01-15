using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Stylesheet Property
    /// </summary>
    /// <remarks>
    /// Properties are always formatted to have a single selector, so it can be used in the backoffice
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public class StylesheetProperty : TracksChangesEntityBase, IValueObject
    {
        private string _alias;
        private string _value;

        public StylesheetProperty(string name, string @alias, string value)
        {
            Name = name;
            _alias = alias;
            _value = value;
        }

        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<StylesheetProperty, string>(x => x.Alias);
        private static readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<StylesheetProperty, string>(x => x.Value);

        /// <summary>
        /// The CSS rule name that can be used by Umbraco in the back office
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This is the CSS Selector
        /// </summary>
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
        /// The CSS value for the selector
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _value = value;
                    return _value;
                }, _value, ValueSelector);
            }
        }

    }
}