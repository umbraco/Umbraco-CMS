using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Linq;

namespace umbraco.cms.businesslogic.macro
{
	/// <summary>
	/// The Macro component are one of the umbraco essentials, used for drawing dynamic content in the public website of umbraco.
	/// 
	/// A Macro is a placeholder for either a xsl transformation, a custom .net control or a .net usercontrol.
	/// 
	/// The Macro is representated in templates and content as a special html element, which are being parsed out and replaced with the
	/// output of either the .net control or the xsl transformation when a page is being displayed to the visitor.
	/// 
	/// A macro can have a variety of properties which are used to transfer userinput to either the usercontrol/custom control or the xsl
	/// 
	/// </summary>
	public class Macro		
	{

		int _id;
		bool _useInEditor;
        bool _renderContent;
        bool _cachePersonalized;
        bool _cacheByPage;

		int _refreshRate;
		string _alias;
		string _name;
		string _assembly;
		string _type;
		string _xslt;
        string _scriptingFile;

		MacroProperty[] _properties;

        bool m_propertiesLoaded = false;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		/// <summary>
		/// id
		/// </summary>
		public int Id 
		{
			get {return _id;}
		}
		
		/// <summary>
		/// If set to true, the macro can be inserted on documents using the richtexteditor.
		/// </summary>
		public bool UseInEditor 
		{
			get {return _useInEditor;}
			set 
			{
				_useInEditor = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroUseInEditor = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		/// <summary>
		/// The cache refreshrate - the maximum amount of time the macro should remain cached in the umbraco
		/// runtime layer.
		/// 
		/// The macro caches are refreshed whenever a document is changed
		/// </summary>
		public int RefreshRate
		{
			get {return _refreshRate;}
			set 
			{
				_refreshRate = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroRefreshRate = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		/// <summary>
		/// The alias of the macro - are used for retrieving the macro when parsing the <?UMBRACO_MACRO></?UMBRACO_MACRO> element,
		/// by using the alias instead of the Id, it's possible to distribute macroes from one installation to another - since the id
		/// is given by an autoincrementation in the database table, and might be used by another macro in the foreing umbraco
		/// </summary>
		public string Alias
		{
			get {return _alias;}
			set 
			{
				_alias = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroAlias = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}
		
		/// <summary>
		/// The userfriendly name
		/// </summary>
		public string Name
		{
			get {return _name;}
			set 
			{
				_name = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroName = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		/// <summary>
		/// If the macro is a wrapper for a custom control, this is the assemly name from which to load the macro
		/// 
		/// specified like: /bin/mydll (without the .dll extension)
		/// </summary>
		public string Assembly
		{
			get {return _assembly;}
			set 
			{
				_assembly = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroScriptAssembly = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		/// <summary>
		/// The relative path to the usercontrol
		/// 
		/// Specified like: /usercontrols/myusercontrol.ascx (with the .ascx postfix)
		/// </summary>
		public string Type
		{
			get {return _type;}
			set 
			{
				_type = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroScriptType = @macroAlias where id = @id", SqlHelper.CreateParameter("@macroAlias", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		/// <summary>
		/// The xsl file used to transform content
		/// 
		/// Umbraco assumes that the xslfile is present in the "/xslt" folder
		/// </summary>
		public string Xslt
		{
			get {return _xslt;}
			set 
			{
				_xslt = value;
				SqlHelper.ExecuteNonQuery("update cmsMacro set macroXSLT = @macroXslt where id = @id", SqlHelper.CreateParameter("@macroXslt", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

        /// <summary>
        /// The python file used to be executed
        /// 
        /// Umbraco assumes that the python file is present in the "/python" folder
        /// </summary>
        public string ScriptingFile {
            get { return _scriptingFile; }
            set {
                _scriptingFile = value;
                SqlHelper.ExecuteNonQuery("update cmsMacro set macroPython = @macroPython where id = @id", SqlHelper.CreateParameter("@macroPython", value), SqlHelper.CreateParameter("@id", this.Id));
            }
        }

        /// <summary>
        /// The python file used to be executed
        /// 
        /// Umbraco assumes that the python file is present in the "/python" folder
        /// </summary>
        public bool RenderContent {
            get { return _renderContent; }
            set {
                _renderContent = value;
                SqlHelper.ExecuteNonQuery("update cmsMacro set macroDontRender = @macroDontRender where id = @id", SqlHelper.CreateParameter("@macroDontRender", !value), SqlHelper.CreateParameter("@id", this.Id));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [cache personalized].
        /// </summary>
        /// <value><c>true</c> if [cache personalized]; otherwise, <c>false</c>.</value>
        public bool CachePersonalized {
            get { return _cachePersonalized; }
            set {
                _cachePersonalized = value;
                SqlHelper.ExecuteNonQuery("update cmsMacro set macroCachePersonalized = @macroCachePersonalized where id = @id", SqlHelper.CreateParameter("@macroCachePersonalized", value), SqlHelper.CreateParameter("@id", this.Id));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the macro is cached for each individual page.
        /// </summary>
        /// <value><c>true</c> if [cache by page]; otherwise, <c>false</c>.</value>
        public bool CacheByPage {
            get { return _cacheByPage; }
            set {
                _cacheByPage = value;
                SqlHelper.ExecuteNonQuery("update cmsMacro set macroCacheByPage = @macroCacheByPage where id = @id", SqlHelper.CreateParameter("@macroCacheByPage", value), SqlHelper.CreateParameter("@id", this.Id));
            }
        }

		/// <summary>
		/// Properties which are used to send parameters to the xsl/usercontrol/customcontrol of the macro
		/// </summary>
		public MacroProperty[] Properties
		{
			get {
                // Add lazy loading
                if (!m_propertiesLoaded)
                {
                    LoadProperties();
                }
                return _properties;
            }
		}

        private void LoadProperties()
        {
            _properties = MacroProperty.GetProperties(Id);
            m_propertiesLoaded = true;
        }

		/// <summary>
		/// Macro initializer
		/// </summary>
		public Macro() {}

		/// <summary>
		/// Macro initializer
		/// </summary>
		/// <param name="Id">The id of the macro</param>
		public Macro(int Id)
		{
			_id = Id;
			setup();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Macro"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public Macro(string alias)
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select macroUseInEditor, macroRefreshRate, macroAlias, macroName, macroScriptType, macroScriptAssembly, macroXSLT, macroPython, macroDontRender, macroCacheByPage, macroCachePersonalized, id from cmsMacro where macroAlias = @alias", SqlHelper.CreateParameter("@alias", alias)))
            {
                if (dr.Read())
                {
                    PopulateMacroInfo(dr);

                    this.LoadProperties();
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format("Alias '{0}' does not match an existing macro", alias));
                }
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
            //event
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            //Stub for now.. 

            if (!e.Cancel) {
                FireAfterSave(e);
            }
        }


		/// <summary>
		/// Deletes the current macro
		/// </summary>
		public void Delete() 
		{
            //event
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) {
                foreach (MacroProperty p in this.Properties)
                    p.Delete();
                SqlHelper.ExecuteNonQuery("delete from cmsMacro where id = @id", SqlHelper.CreateParameter("@id", this._id));

                FireAfterDelete(e);
            }
        }

        public static Macro Import(XmlNode n)
        {

            Macro m = null;
            string alias = xmlHelper.GetNodeValue(n.SelectSingleNode("alias"));
            try
            {
                //check to see if the macro alreay exists in the system
                //it's better if it does and we keep using it, alias *should* be unique remember
                m = new Macro(alias);
                Macro.GetByAlias(alias);
            }
            catch (IndexOutOfRangeException)
            {
                m = MakeNew(xmlHelper.GetNodeValue(n.SelectSingleNode("name")));
            }

            try
            {
                m.Alias = alias;
                m.Assembly = xmlHelper.GetNodeValue(n.SelectSingleNode("scriptAssembly"));
                m.Type = xmlHelper.GetNodeValue(n.SelectSingleNode("scriptType"));
                m.Xslt = xmlHelper.GetNodeValue(n.SelectSingleNode("xslt"));
                m.RefreshRate = int.Parse(xmlHelper.GetNodeValue(n.SelectSingleNode("refreshRate")));

                if (n.SelectSingleNode("scriptingFile") != null)
                    m.ScriptingFile = xmlHelper.GetNodeValue(n.SelectSingleNode("scriptingFile"));

                try
                {
                    m.UseInEditor = bool.Parse(xmlHelper.GetNodeValue(n.SelectSingleNode("useInEditor")));
                }
                catch (Exception macroExp)
                {
                    BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), -1, "Error creating macro property: " + macroExp.ToString());
                }

                // macro properties
                foreach (XmlNode mp in n.SelectNodes("properties/property"))
                {
                    try
                    {
                        string propertyAlias = mp.Attributes.GetNamedItem("alias").Value;
                        var property = m.Properties.SingleOrDefault(p => p.Alias == propertyAlias);
                        if (property != null)
                        {
                            property.Public = bool.Parse(mp.Attributes.GetNamedItem("show").Value);
                            property.Name = mp.Attributes.GetNamedItem("name").Value;
                            property.Type = new MacroPropertyType(mp.Attributes.GetNamedItem("propertyType").Value);

                            property.Save();
                        }
                        else
                        {
                            MacroProperty.MakeNew(
                                m,
                                bool.Parse(mp.Attributes.GetNamedItem("show").Value),
                                propertyAlias,
                                mp.Attributes.GetNamedItem("name").Value,
                                new MacroPropertyType(mp.Attributes.GetNamedItem("propertyType").Value)
                            );
                        }
                    }
                    catch (Exception macroPropertyExp)
                    {
                        BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), -1, "Error creating macro property: " + macroPropertyExp.ToString());
                    }
                }

                m.Save();
            }
            catch { return null; }

            return m;
        }

		private void setup() 
		{
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select id, macroUseInEditor, macroRefreshRate, macroAlias, macroName, macroScriptType, macroScriptAssembly, macroXSLT, macroPython, macroDontRender, macroCacheByPage, macroCachePersonalized  from cmsMacro where id = @id", SqlHelper.CreateParameter("@id", _id)))
			{
                if (dr.Read())
                {
                    PopulateMacroInfo(dr);
                }
                else
                {
                    throw new ArgumentException("No macro found for the id specified");
                }
			}
		}

        private void PopulateMacroInfo(IRecordsReader dr)
        {
            _useInEditor = dr.GetBoolean("macroUseInEditor");
            _refreshRate = dr.GetInt("macroRefreshRate");
            _alias = dr.GetString("macroAlias");
            _id = dr.GetInt("id");
            _name = dr.GetString("macroName");
            _assembly = dr.GetString("macroScriptAssembly");
            _type = dr.GetString("macroScriptType");
            _xslt = dr.GetString("macroXSLT");
            _scriptingFile = dr.GetString("macroPython");

            _cacheByPage = dr.GetBoolean("macroCacheByPage");
            _cachePersonalized = dr.GetBoolean("macroCachePersonalized");
            _renderContent = !dr.GetBoolean("macroDontRender");
        }

		/// <summary>
		/// Get an xmlrepresentation of the macro, used for exporting the macro to a package for distribution
		/// </summary>
		/// <param name="xd">Current xmldocument context</param>
		/// <returns>An xmlrepresentation of the macro</returns>
		public XmlNode ToXml(XmlDocument xd) {

            XmlNode doc = xd.CreateElement("macro");

			// info section
			doc.AppendChild(xmlHelper.addTextNode(xd, "name", this.Name));
			doc.AppendChild(xmlHelper.addTextNode(xd, "alias", this.Alias));
			doc.AppendChild(xmlHelper.addTextNode(xd, "scriptType", this.Type));
			doc.AppendChild(xmlHelper.addTextNode(xd, "scriptAssembly", this.Assembly));
			doc.AppendChild(xmlHelper.addTextNode(xd, "xslt", this.Xslt));
			doc.AppendChild(xmlHelper.addTextNode(xd, "useInEditor", this.UseInEditor.ToString()));
			doc.AppendChild(xmlHelper.addTextNode(xd, "refreshRate", this.RefreshRate.ToString()));
            doc.AppendChild(xmlHelper.addTextNode(xd, "scriptingFile", this.ScriptingFile));

			// properties
            XmlNode props = xd.CreateElement("properties");
			foreach (MacroProperty p in this.Properties)
				props.AppendChild(p.ToXml(xd));
			doc.AppendChild(props);

			return doc;
		}

        public void RefreshProperties()
        {
            m_propertiesLoaded = false;
        }


		#region STATICS

		/// <summary>
		/// Creates a new macro given the name
		/// </summary>
		/// <param name="Name">Userfriendly name</param>
		/// <returns>The newly macro</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
		public static Macro MakeNew(string Name) 
		{
            int macroId = 0;
            // The method is synchronized
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsMacro (macroAlias, macroName) values (@macroAlias, @macroName)",
                SqlHelper.CreateParameter("@macroAlias", Name.Replace(" ", String.Empty)),
                SqlHelper.CreateParameter("@macroName", Name));
            macroId = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsMacro");

            Macro newMacro = new Macro(macroId);
           
            //fire new event
            NewEventArgs e = new NewEventArgs();
            newMacro.OnNew(e);
            
            return newMacro;
		}

		/// <summary>
		/// Retrieve all macroes
		/// </summary>
		/// <returns>A list of all macroes</returns>
		public static Macro[] GetAll() 
		{
			// zb-00001 #29927 : refactor
			IRecordsReader dr = SqlHelper.ExecuteReader("select id from cmsMacro order by macroName");
			var list = new System.Collections.Generic.List<Macro>();
			while (dr.Read()) list.Add(new Macro(dr.GetInt("id")));
			return list.ToArray();
		}

		/// <summary>
		/// Static contructor for retrieving a macro given an alias
		/// </summary>
		/// <param name="Alias">The alias of the macro</param>
		/// <returns>If the macro with the given alias exists, it returns the macro, else null</returns>
		[Obsolete("Use the alias constructor")]
        public static Macro GetByAlias(string Alias) 
		{
			try 
			{
				return new Macro(Alias);
			} 
			catch 
			{
				return null;
			}
		}

        public static MacroTypes FindMacroType(string xslt, string scriptFile, string scriptType, string scriptAssembly)
        {
            if (!string.IsNullOrEmpty(xslt))
                return MacroTypes.XSLT;
            else
            {
                if (!string.IsNullOrEmpty(scriptFile))
                    return MacroTypes.Script;
                else
                {
                    if (!string.IsNullOrEmpty(scriptType) && scriptType.ToLower().IndexOf(".ascx") > -1)
                    {
                        return MacroTypes.UserControl;
                    }
                    else if (!string.IsNullOrEmpty(scriptType) && !string.IsNullOrEmpty(scriptAssembly))
                        return MacroTypes.CustomControl;
                }
            }

            return MacroTypes.Unknown;
        }

        public static string GenerateCacheKeyFromCode(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException("input", "An MD5 hash cannot be generated when 'input' parameter is null!");

            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }



        //Macro events

        //Delegates
        public delegate void SaveEventHandler(Macro sender, SaveEventArgs e);
        public delegate void NewEventHandler(Macro sender, NewEventArgs e);
        public delegate void DeleteEventHandler(Macro sender, DeleteEventArgs e);

        /// <summary>
        /// Occurs when a macro is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
		#endregion
	}
}
