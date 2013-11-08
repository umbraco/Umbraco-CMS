using System;
using System.Data;
using System.Xml;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core;


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

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
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
        [Obsolete]
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
            ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<MacroPropertyDto>(
                "select macro, macroPropertyHidden, macroPropertyType, macroPropertySortOrder, macroPropertyAlias, macroPropertyName from cmsMacroProperty where id = @0", _id)
                .IfNull(x => { throw new ArgumentException(string.Format("No macro property found with the specified id = '{0}'", Id)); })
                .IfNotNull<MacroPropertyDto>(x => PopulateMacroPropertyFromDto(x));
        }

        /// <summary>
        /// Deletes the current macroproperty
        /// </summary>
        public void Delete()
        {
            ApplicationContext.Current.DatabaseContext.Database.Execute("delete from cmsMacroProperty where id = @0", this._id);  
        }

        public void Save()
        {
            if (_id == 0)
            {
                MacroProperty mp = MakeNew(m_macro, Public, Alias, Name, Type);
                _id = mp.Id;
            }
            else
            {
                ApplicationContext.Current.DatabaseContext.Database.Update(PopulateDtoFromMacroProperty(this));
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
            var props = new List<MacroProperty>();
            ApplicationContext.Current.DatabaseContext.Database.Fetch<int>("select id from cmsMacroProperty where macro = @0 order by macroPropertySortOrder, id ASC", MacroId)
                 .ForEach<int>(x => props.Add(new MacroProperty(x)));
            return props.ToArray();
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
            var macroPropertyDto = new MacroPropertyDto()
            {
                Id = 0,
                Hidden = show,
                Type = (short)propertyType.Id,  
                Macro = M.Id,
                //SortOrder => default => 0,  
                Alias = alias, 
                Name = name    
            };
            ApplicationContext.Current.DatabaseContext.Database.Insert(macroPropertyDto);  

            macroPropertyId = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsMacroProperty");
            return new MacroProperty(macroPropertyId);
        }

        #endregion

        private void PopulateMacroPropertyFromDto(MacroPropertyDto dto)
        {
            m_macro = new Macro(dto.Macro);
            _public = dto.Hidden; // should it be !dto.Hidden? But original was dr.GetBoolean("macroPropertyHidden");
            _sortOrder = dto.SortOrder;
            _alias = dto.Alias;
            _name = dto.Name;
            _type = new MacroPropertyType(dto.Type);
        }
        private MacroPropertyDto PopulateDtoFromMacroProperty(MacroProperty p)
        {
            return new MacroPropertyDto()
            {
                 Macro = p.Macro.Id,
                 Id = p.Id,
                 Hidden = p.Public,
                 Alias = p.Alias,
                 Name = p.Name,
                 Type = (short)p.Type.Id,
                 SortOrder = (byte)p.SortOrder
            };
        }
    }
}
