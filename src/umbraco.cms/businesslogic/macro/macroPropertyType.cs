using System;
using System.Data;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.macro
{
    /// <summary>
    /// The MacroPropertyType class contains information on the assembly and class of the 
    /// IMacroGuiRendering component and basedatatype
    /// </summary>
    [Obsolete("This class is no longer used, the cmsMacroPropertyType has been removed, all methods will return empty collections and not perform any functions")]
    public class MacroPropertyType
    {
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public static List<MacroPropertyType> GetAll
        {
            get { return new List<MacroPropertyType>(); }
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The alias of the MacroPropertyType
        /// </summary>
        public string Alias { get; internal set; }

        /// <summary>
        /// The assembly (without the .dll extension) used to retrieve the component at runtime
        /// </summary>
        public string Assembly { get; internal set; }

        /// <summary>
        /// The MacroPropertyType
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// The IMacroGuiRendering component (namespace.namespace.Classname)
        /// </summary>
        public string BaseType { get; internal set; }
        
        internal MacroPropertyType()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Id">Identifier</param>
        public MacroPropertyType(int Id)
        {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Alias">The alias of the MacroPropertyType</param>
        public MacroPropertyType(string Alias)
        {
           
        }

    }
}
