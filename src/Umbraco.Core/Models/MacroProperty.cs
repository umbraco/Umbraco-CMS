using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro Property
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MacroProperty : IMacroProperty
    {
        /// <summary>
        /// Gets or sets the Alias of the Property
        /// </summary>
        [DataMember]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Property
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Sort Order of the Property
        /// </summary>
        [DataMember]
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the Type for this Property
        /// </summary>
        /// <remarks>
        /// The MacroPropertyTypes acts as a plugin for Macros.
        /// All types was previously contained in the database, but has been ported to code.
        /// </remarks>
        [DataMember]
        public IMacroPropertyType PropertyType { get; set; }
    }
}