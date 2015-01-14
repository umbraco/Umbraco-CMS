using System;
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
    public class StylesheetProperty : IValueObject
    {
        public StylesheetProperty(string @alias, string value)
        {
            Alias = alias;
            Value = value;
        }

        public string Alias { get; set; }
        public string Value { get; set; }
        
        //public bool IsPartOfAtRule { get; set; }
        ////This should never be used, it's here because we still have stylesheets based on DB ids and 
        //// we need to wrap the legacy classes
        //internal int Id { get; set; }
    }
}