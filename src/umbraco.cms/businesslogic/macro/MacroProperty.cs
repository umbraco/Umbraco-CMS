using System;
using System.Data;
using System.Xml;
using System.Runtime.CompilerServices;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core.Xml;


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
    [Obsolete("This is no longer used, use the IMacroService and related models instead")]
    public class MacroProperty
    {
        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return LegacySqlHelper.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroProperty"/> class.
        /// </summary>
        public MacroProperty()
        {
            Key = Guid.NewGuid();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Id">Id</param>
        public MacroProperty(int Id)
        {
            this.Id = Id;
            Setup();
        }

        /// <summary>
        /// The sortorder
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// This is not used for anything
        /// </summary>
        [Obsolete("This is not used for anything and will be removed in future versions")]
        public bool Public { get; set; }

        /// <summary>
        /// The macro property alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The userfriendly name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the macro.
        /// </summary>
        /// <value>The macro.</value>
        public Macro Macro { get; set; }

        private string _parameterEditorAlias;

        /// <summary>
        /// The macro parameter editor alias used to render the editor
        /// </summary>
        public string ParameterEditorAlias
        {
            get { return _parameterEditorAlias; }
            set
            {
                //try to get the new mapped parameter editor
                var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(value, false);
                if (mapped.IsNullOrWhiteSpace() == false)
                {
                    _parameterEditorAlias = mapped;
                }
                else
                {
                    _parameterEditorAlias = value;    
                }
                
            }
        }

        private void Setup()
        {
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (var dr = sqlHelper.ExecuteReader("select uniqueId, macro, editorAlias, macroPropertySortOrder, macroPropertyAlias, macroPropertyName from cmsMacroProperty where id = @id", sqlHelper.CreateParameter("@id", Id)))
            {
                if (dr.Read())
                {
                    Key = dr.GetGuid("uniqueId");
                    Macro = new Macro(dr.GetInt("macro"));
                    SortOrder = (int)dr.GetByte("macroPropertySortOrder");
                    Alias = dr.GetString("macroPropertyAlias");
                    Name = dr.GetString("macroPropertyName");
                    ParameterEditorAlias = dr.GetString("editorAlias");
                }
                else
                {
                    throw new ArgumentException("No macro property found for the id specified");
                }
            }
        }

        /// <summary>
        /// Deletes the current macroproperty
        /// </summary>
        public void Delete()
        {
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
                sqlHelper.ExecuteNonQuery("delete from cmsMacroProperty where id = @id", sqlHelper.CreateParameter("@id", this.Id));
        }

        public void Save()
        {
            if (Id == 0)
            {
                MacroProperty mp = MakeNew(Macro, Alias, Name, ParameterEditorAlias);
                Id = mp.Id;
            }
            else
            {
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
                    sqlHelper.ExecuteNonQuery("UPDATE cmsMacroProperty set macro = @macro, " +
                                          "macropropertyAlias = @alias, macroPropertyName = @name, " +
                                          "editorAlias = @editorAlias, macroPropertySortOrder = @so WHERE id = @id",
                                          sqlHelper.CreateParameter("@id", Id),
                                          sqlHelper.CreateParameter("@macro", Macro.Id),
                                          sqlHelper.CreateParameter("@alias", Alias),
                                          sqlHelper.CreateParameter("@name", Name),
                                          sqlHelper.CreateParameter("@editorAlias", ParameterEditorAlias),
                                          sqlHelper.CreateParameter("@so", SortOrder));
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

            doc.Attributes.Append(XmlHelper.AddAttribute(xd, "name", this.Name));
            doc.Attributes.Append(XmlHelper.AddAttribute(xd, "alias", this.Alias));
            doc.Attributes.Append(XmlHelper.AddAttribute(xd, "propertyType", this.ParameterEditorAlias));

            return doc;
        }

        #region STATICS

        /// <summary>
        /// Retieve all MacroProperties of a macro
        /// </summary>
        /// <param name="macroId">Macro identifier</param>
        /// <returns>All MacroProperties of a macro</returns>
        public static MacroProperty[] GetProperties(int macroId)
        {
            var props = new List<MacroProperty>();
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (IRecordsReader dr = sqlHelper.ExecuteReader("select id from cmsMacroProperty where macro = @macroId order by macroPropertySortOrder, id ASC", sqlHelper.CreateParameter("@macroId", macroId)))
            {                
                while (dr.Read())
                {
                    props.Add(new MacroProperty(dr.GetInt("id")));
                }
                return props.ToArray();
            }
        }

        
        /// <summary>
        /// Creates a new MacroProperty on a macro
        /// </summary>
        /// <param name="macro">The macro</param>
        /// <param name="alias">The alias of the property</param>
        /// <param name="name">Userfriendly MacroProperty name</param>
        /// <param name="editorAlias">The Alias of the parameter editor</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static MacroProperty MakeNew(Macro macro, string alias, string name, string editorAlias)
        {
            //try to get the new mapped parameter editor
            var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(editorAlias, false);
            if (mapped.IsNullOrWhiteSpace() == false)
            {
                editorAlias = mapped;
            }

            int macroPropertyId = 0;
            // The method is synchronized
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "INSERT INTO cmsMacroProperty (macro, macropropertyAlias, macroPropertyName, editorAlias) VALUES (@macro, @alias, @name, @editorAlias)",
                    sqlHelper.CreateParameter("@macro", macro.Id),
                    sqlHelper.CreateParameter("@alias", alias),
                    sqlHelper.CreateParameter("@name", name),
                    sqlHelper.CreateParameter("@editorAlias", editorAlias));
                macroPropertyId = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsMacroProperty");
                return new MacroProperty(macroPropertyId);
            }
        }

        #endregion

    }
}
