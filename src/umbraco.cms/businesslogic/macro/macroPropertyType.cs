using System;
using System.Data;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.macro
{
    /// <summary>
    /// The MacroPropertyType class contains information on the assembly and class of the 
    /// IMacroGuiRendering component and basedatatype
    /// </summary>
    public class MacroPropertyType
    {
        int _id;
        string _alias;
        string _assembly;
        string _type;
        string _baseType;
        private static List<MacroPropertyType> m_allPropertyTypes = new List<MacroPropertyType>();

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public static List<MacroPropertyType> GetAll
        {
            get
            {
                if (m_allPropertyTypes.Count == 0)
                    ApplicationContext.Current.DatabaseContext.Database.Fetch<int>("select id from cmsMacroPropertyType order by macroPropertyTypeAlias")
                         .ForEach<int>(x => m_allPropertyTypes.Add(new MacroPropertyType(x)) );

                return m_allPropertyTypes; 
            }
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// The alias of the MacroPropertyType
        /// </summary>
        public string Alias { get { return _alias; } }

        /// <summary>
        /// The assembly (without the .dll extension) used to retrieve the component at runtime
        /// </summary>
        public string Assembly { get { return _assembly; } }

        /// <summary>
        /// The MacroPropertyType
        /// </summary>
        public string Type { get { return _type; } }

        /// <summary>
        /// The IMacroGuiRendering component (namespace.namespace.Classname)
        /// </summary>
        public string BaseType { get { return _baseType; } }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Id">Identifier</param>
        public MacroPropertyType(int Id)
        {
            _id = Id;
            setup();
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Alias">The alias of the MacroPropertyType</param>
        public MacroPropertyType(string Alias)
        {
            var dto = ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<MacroPropertyTypeDto>("select id from cmsMacroPropertyType where macroPropertyTypeAlias = @0", Alias);
            if (dto == null) throw new ArgumentException(string.Format("No macro property type found with the specified Alias  = '{0}'", Alias));
            _id = dto.Id; 
            setup();
        }

        private void setup()
        {
            ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<MacroPropertyTypeDto>(
                "select macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType from cmsMacroPropertyType where id = @0", _id)
                .IfNull(x => { throw new ArgumentException("No macro property type found with the id specified"); })
                .IfNotNull<MacroPropertyTypeDto>(x =>
                {
                    _alias = x.Alias;
                    _assembly = x.RenderAssembly;
                    _type = x.RenderType;
                    _baseType = x.BaseType;
                });               
        }

    }
}
