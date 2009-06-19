using System;
using System.Data;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.macro {
    /// <summary>
    /// The MacroPropertyType class contains information on the assembly and class of the 
    /// IMacroGuiRendering component and basedatatype
    /// 
    /// TODO: implement interface/abstract factory pattern
    /// </summary>
    public class MacroPropertyType {
        int _id;
        string _alias;
        string _assembly;
        string _type;
        string _baseType;
        private static List<MacroPropertyType> m_allPropertyTypes = new List<MacroPropertyType>();

        protected static ISqlHelper SqlHelper {
            get { return Application.SqlHelper; }
        }

        public static List<MacroPropertyType> GetAll {
            get {
                if (m_allPropertyTypes.Count == 0) {
                    using (IRecordsReader dr = SqlHelper.ExecuteReader("select id from cmsMacroPropertyType order by macroPropertyTypeAlias")) {
                        while (dr.Read()) {
                            m_allPropertyTypes.Add(new MacroPropertyType(dr.GetShort("id")));
                        }
                    }
                }

                return m_allPropertyTypes;
            }
        }


        /// <summary>
        /// Identifier
        /// </summary>
        public int Id {
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
        public MacroPropertyType(int Id) {
            _id = Id;
            setup();
        }

        public MacroPropertyType() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Alias">The alias of the MacroPropertyType</param>
        public MacroPropertyType(string Alias) {
            _id = SqlHelper.ExecuteScalar<int>("select id from cmsMacroPropertyType where macroPropertyTypeAlias = @alias", SqlHelper.CreateParameter("@alias", Alias));
            setup();
        }

        private void setup() {
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType from cmsMacroPropertyType where id = @id", SqlHelper.CreateParameter("@id", _id))) {
                if (dr.Read()) {
                    _alias = dr.GetString("macroPropertyTypeAlias");
                    _assembly = dr.GetString("macroPropertyTypeRenderAssembly");
                    _type = dr.GetString("macroPropertyTypeRenderType");
                    _baseType = dr.GetString("macroPropertyTypeBaseType");
                }
            }
        }


    }
}
