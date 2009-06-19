using System;
using System.Data;
using System.Xml;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;


namespace umbraco.cms.businesslogic.macro
{
    /// <summary>
    /// The macro property is used by macroes to communicate/transfer userinput to an instance of a macro.
    /// 
    /// It contains information on which type of data is inputted aswell as the userinterface used to input data
    /// 
    /// A MacroProperty uses it's MacroPropertyType to define which underlaying component should be used when
    /// rendering the MacroProperty editor aswell as which datatype its containing.
    /// </summary>
    public class MacroProperty
    {

        int _id;
        int _sortOrder;
        bool _public;
        string _alias;
        string _name;
        cms.businesslogic.macro.Macro m_macro;
        cms.businesslogic.macro.MacroPropertyType _type;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroProperty"/> class.
        /// </summary>
        public MacroProperty()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Id">Id</param>
        public MacroProperty(int Id)
        {
            _id = Id;
            setup();
        }

        /// <summary>
        /// The sortorder
        /// </summary>
        public int SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        /// <summary>
        /// If set to true, the user will be presented with an editor to input data.
        /// 
        /// If not, the field can be manipulated by a default value given by the MacroPropertyType, this is s
        /// </summary>
        public bool Public
        {
            get { return _public; }
            set { _public = value; }
        }

        /// <summary>
        /// The alias if of the macroproperty, this is used in the special macro element
        /// <?UMBRACO_MACRO macroAlias="value"></?UMBRACO_MACRO>
        /// 
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>
        /// The userfriendly name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets or sets the macro.
        /// </summary>
        /// <value>The macro.</value>
        public Macro Macro
        {
            get { return m_macro; }
            set { m_macro = value; }
        }

        /// <summary>
        /// The basetype which defines which component is used in the UI for editing content
        /// </summary>
        public MacroPropertyType Type
        {
            get { return _type; }
            set { _type = value; }
        }


        private void setup()
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select macro, macroPropertyHidden, macroPropertyType, macroPropertySortOrder, macroPropertyAlias, macroPropertyName from cmsMacroProperty where id = @id", SqlHelper.CreateParameter("@id", _id)))
            {
                if (dr.Read())
                {
                    m_macro = new Macro(dr.GetInt("macro"));
                    _public = dr.GetBoolean("macroPropertyHidden");
                    _sortOrder = (int)dr.GetByte("macroPropertySortOrder");
                    _alias = dr.GetString("macroPropertyAlias");
                    _name = dr.GetString("macroPropertyName");
                    _type = new MacroPropertyType(dr.GetShort("macroPropertyType"));
                }
            }
        }

        /// <summary>
        /// Deletes the current macroproperty
        /// </summary>
        public void Delete()
        {
            SqlHelper.ExecuteNonQuery("delete from cmsMacroProperty where id = @id", SqlHelper.CreateParameter("@id", this._id));
        }

        public void Save()
        {
            if (_id == 0)
            {
                MacroProperty mp =
                    MakeNew(m_macro, Public, Alias, Name, Type);
                _id = mp.Id;

            }
            else
            {
                SqlHelper.ExecuteNonQuery("UPDATE cmsMacroProperty set macro = @macro, macroPropertyHidden = @show, macropropertyAlias = @alias, macroPropertyName = @name, macroPropertyType = @type WHERE id = @id",
    SqlHelper.CreateParameter("@id", Id),
    SqlHelper.CreateParameter("@macro", Macro.Id),
    SqlHelper.CreateParameter("@show", Public),
    SqlHelper.CreateParameter("@alias", Alias),
    SqlHelper.CreateParameter("@name", Name),
    SqlHelper.CreateParameter("@type", Type.Id));
            }
        }

        /// <summary>
        /// Retrieve a Xmlrepresentation of the MacroProperty used for exporting the Macro to the package
        /// </summary>
        /// <param name="xd">XmlDocument context</param>
        /// <returns>A xmlrepresentation of the object</returns>
        public XmlElement ToXml(XmlDocument xd)
        {
            XmlElement doc = xd.CreateElement("property");

            doc.Attributes.Append(xmlHelper.addAttribute(xd, "name", this.Name));
            doc.Attributes.Append(xmlHelper.addAttribute(xd, "alias", this.Alias));
            doc.Attributes.Append(xmlHelper.addAttribute(xd, "show", this.Public.ToString()));
            doc.Attributes.Append(xmlHelper.addAttribute(xd, "propertyType", this.Type.Alias));

            return doc;
        }

        #region STATICS

        /// <summary>
        /// Retieve all MacroProperties of a macro
        /// </summary>
        /// <param name="MacroId">Macro identifier</param>
        /// <returns>All MacroProperties of a macro</returns>
        public static MacroProperty[] GetProperties(int MacroId)
        {
            int totalProperties = SqlHelper.ExecuteScalar<int>("select count(*) from cmsMacroProperty where macro = @macroID", SqlHelper.CreateParameter("@macroID", MacroId));
            int count = 0;
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select id from cmsMacroProperty where macro = @macroId order by macroPropertySortOrder, id ASC", SqlHelper.CreateParameter("@macroId", MacroId)))
            {
                MacroProperty[] retval = new MacroProperty[totalProperties];
                while (dr.Read())
                {
                    retval[count] = new MacroProperty(dr.GetInt("id"));
                    count++;
                }
                return retval;
            }
        }

        /// <summary>
        /// Creates a new MacroProperty on a macro
        /// </summary>
        /// <param name="M">The macro</param>
        /// <param name="show">Will the editor be able to input data</param>
        /// <param name="alias">The alias of the property</param>
        /// <param name="name">Userfriendly MacroProperty name</param>
        /// <param name="propertyType">The MacroPropertyType of the property</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static MacroProperty MakeNew(Macro M, bool show, string alias, string name, MacroPropertyType propertyType)
        {
            int macroPropertyId = 0;
            // The method is synchronized
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsMacroProperty (macro, macroPropertyHidden, macropropertyAlias, macroPropertyName, macroPropertyType) VALUES (@macro, @show, @alias, @name, @type)",
                SqlHelper.CreateParameter("@macro", M.Id),
                SqlHelper.CreateParameter("@show", show),
                SqlHelper.CreateParameter("@alias", alias),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@type", propertyType.Id));
            macroPropertyId = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsMacroProperty");
            return new MacroProperty(macroPropertyId);
        }

        #endregion

    }
}
