using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Runtime.CompilerServices;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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
    [Obsolete("This is no longer used, use the IMacroService and related models instead")]
	public class Macro
	{
        //initialize empty model
	    internal IMacro MacroEntity = new Umbraco.Core.Models.Macro();

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		/// <summary>
		/// id
		/// </summary>
		public int Id 
		{
			get { return MacroEntity.Id; }
		}
		
		/// <summary>
		/// If set to true, the macro can be inserted on documents using the richtexteditor.
		/// </summary>
		public bool UseInEditor 
		{
            get { return MacroEntity.UseInEditor; }
			set { MacroEntity.UseInEditor = value; }
		}

		/// <summary>
		/// The cache refreshrate - the maximum amount of time the macro should remain cached in the umbraco
		/// runtime layer.
		/// 
		/// The macro caches are refreshed whenever a document is changed
		/// </summary>
		public int RefreshRate
		{
            get { return MacroEntity.CacheDuration; }
			set { MacroEntity.CacheDuration = value; }
		}

        /// <summary>
		/// The alias of the macro - are used for retrieving the macro when parsing the {?UMBRACO_MACRO}{/?UMBRACO_MACRO} element,
		/// by using the alias instead of the Id, it's possible to distribute macroes from one installation to another - since the id
		/// is given by an autoincrementation in the database table, and might be used by another macro in the foreing umbraco
        /// </summary>
		public string Alias
		{
			get { return MacroEntity.Alias; }
			set { MacroEntity.Alias = value; }
		}
		
		/// <summary>
		/// The userfriendly name
		/// </summary>
		public string Name
		{
            get { return MacroEntity.Name; }
            set { MacroEntity.Name = value; }
		}

		/// <summary>
		/// If the macro is a wrapper for a custom control, this is the assemly name from which to load the macro
		/// 
		/// specified like: /bin/mydll (without the .dll extension)
		/// </summary>
		public string Assembly
		{
            get { return MacroEntity.ControlAssembly; }
            set { MacroEntity.ControlAssembly = value; }
		}

		/// <summary>
		/// The relative path to the usercontrol or the assembly type of the macro when using .Net custom controls
		/// </summary>
		/// <remarks>
		/// When using a user control the value is specified like: /usercontrols/myusercontrol.ascx (with the .ascx postfix)
		/// </remarks>
		public string Type
		{
            get { return MacroEntity.ControlType; }
            set { MacroEntity.ControlType = value; }
		}

		/// <summary>
		/// The xsl file used to transform content
		/// 
		/// Umbraco assumes that the xslfile is present in the "/xslt" folder
		/// </summary>
		public string Xslt
		{
            get { return MacroEntity.XsltPath; }
            set { MacroEntity.XsltPath = value; }
		}

	    /// <summary>
	    /// This field is used to store the file value for any scripting macro such as python, ruby, razor macros or Partial View Macros        
	    /// </summary>
	    /// <remarks>
	    /// Depending on how the file is stored depends on what type of macro it is. For example if the file path is a full virtual path
	    /// starting with the ~/Views/MacroPartials then it is deemed to be a Partial View Macro, otherwise the file extension of the file
	    /// saved will determine which macro engine will be used to execute the file.
	    /// </remarks>
	    public string ScriptingFile
	    {
	        get { return MacroEntity.ScriptPath; }
            set { MacroEntity.ScriptPath = value; }
	    }

	    /// <summary>
	    /// The python file used to be executed
	    /// 
	    /// Umbraco assumes that the python file is present in the "/python" folder
	    /// </summary>
	    public bool RenderContent
	    {
            get { return MacroEntity.DontRender == false; }
            set { MacroEntity.DontRender = value == false; }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether [cache personalized].
	    /// </summary>
	    /// <value><c>true</c> if [cache personalized]; otherwise, <c>false</c>.</value>
	    public bool CachePersonalized
	    {
            get { return MacroEntity.CacheByMember; }
            set { MacroEntity.CacheByMember = value; }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the macro is cached for each individual page.
	    /// </summary>
	    /// <value><c>true</c> if [cache by page]; otherwise, <c>false</c>.</value>
	    public bool CacheByPage
	    {
            get { return MacroEntity.CacheByPage; }
            set { MacroEntity.CacheByPage = value; }
	    }

	    /// <summary>
	    /// Properties which are used to send parameters to the xsl/usercontrol/customcontrol of the macro
	    /// </summary>
	    public MacroProperty[] Properties
	    {
	        get
	        {
	            return MacroEntity.Properties.Select(x => new MacroProperty
	                {
	                    Alias = x.Alias,
	                    Name = x.Name,
                        SortOrder = x.SortOrder,
                        Macro = this,
                        ParameterEditorAlias = x.EditorAlias
	                }).ToArray();
	        }
	    }
        
		/// <summary>
		/// Macro initializer
		/// </summary>
		public Macro()
		{
		}

		/// <summary>
		/// Macro initializer
		/// </summary>
		/// <param name="Id">The id of the macro</param>
		public Macro(int Id)
		{
            Setup(Id);
		}

        internal Macro(IMacro macro)
        {
            MacroEntity = macro;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Macro"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public Macro(string alias)
        {
            Setup(alias);
        }

	    /// <summary>
	    /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
	    /// </summary>
	    public virtual void Save()
	    {
	        //event
	        var e = new SaveEventArgs();
	        FireBeforeSave(e);

	        if (e.Cancel == false)
	        {
	            ApplicationContext.Current.Services.MacroService.Save(MacroEntity);

	            FireAfterSave(e);
	        }
	    }

	    /// <summary>
		/// Deletes the current macro
		/// </summary>
		public void Delete() 
		{
            //event
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);

		    if (e.Cancel == false)
		    {
		        ApplicationContext.Current.Services.MacroService.Delete(MacroEntity);

		        FireAfterDelete(e);
		    }
		}

        //TODO: Fix this, this should wrap a new API!

        public static Macro Import(XmlNode n)
        {
            var alias = XmlHelper.GetNodeValue(n.SelectSingleNode("alias"));
            //check to see if the macro alreay exists in the system
            //it's better if it does and we keep using it, alias *should* be unique remember
            
            var m = Macro.GetByAlias(alias);


            if (m == null)
            {
                m = MakeNew(XmlHelper.GetNodeValue(n.SelectSingleNode("name")));
            }
            try
            {
                m.Alias = alias;
                m.Assembly = XmlHelper.GetNodeValue(n.SelectSingleNode("scriptAssembly"));
                m.Type = XmlHelper.GetNodeValue(n.SelectSingleNode("scriptType"));
                m.Xslt = XmlHelper.GetNodeValue(n.SelectSingleNode("xslt"));
                m.RefreshRate = int.Parse(XmlHelper.GetNodeValue(n.SelectSingleNode("refreshRate")));

                // we need to validate if the usercontrol is missing the tilde prefix requirement introduced in v6
                if (string.IsNullOrEmpty(m.Assembly) && string.IsNullOrEmpty(m.Type) == false && m.Type.StartsWith("~") == false)
                {
                    m.Type = "~/" + m.Type;
                }

                if (n.SelectSingleNode("scriptingFile") != null)
                {
                    m.ScriptingFile = XmlHelper.GetNodeValue(n.SelectSingleNode("scriptingFile"));
                }

                try
                {
                    m.UseInEditor = bool.Parse(XmlHelper.GetNodeValue(n.SelectSingleNode("useInEditor")));
                }
                catch (Exception macroExp)
                {
                    LogHelper.Error<Macro>("Error creating macro property", macroExp);
                }

                // macro properties
                foreach (XmlNode mp in n.SelectNodes("properties/property"))
                {
                    try
                    {
                        var propertyAlias = mp.Attributes.GetNamedItem("alias").Value;
                        var property = m.Properties.SingleOrDefault(p => p.Alias == propertyAlias);
                        if (property != null)
                        {
                            property.Name = mp.Attributes.GetNamedItem("name").Value;
                            property.ParameterEditorAlias = mp.Attributes.GetNamedItem("propertyType").Value;

                            property.Save();
                        }
                        else
                        {
                            MacroProperty.MakeNew(
                                m,
                                propertyAlias,
                                mp.Attributes.GetNamedItem("name").Value,
                                mp.Attributes.GetNamedItem("propertyType").Value
                                );
                        }
                    }
                    catch (Exception macroPropertyExp)
                    {
                        LogHelper.Error<Macro>("Error creating macro property", macroPropertyExp);
                    }
                }

                m.Save();
            }
            catch (Exception ex)
            {
                LogHelper.Error<Macro>("An error occurred importing a macro", ex);
                return null;
            }

            return m;
        }

		private void Setup(int id)
		{
            var macro = ApplicationContext.Current.Services.MacroService.GetById(id);

            if (macro == null)
                throw new ArgumentException(string.Format("No Macro exists with id '{0}'", id));

		    MacroEntity = macro;
		}

        private void Setup(string alias)
        {
            var macro = ApplicationContext.Current.Services.MacroService.GetByAlias(alias);

            if (macro == null)
                throw new ArgumentException(string.Format("No Macro exists with alias '{0}'", alias));

            MacroEntity = macro;
        }

	    /// <summary>
	    /// Get an xmlrepresentation of the macro, used for exporting the macro to a package for distribution
	    /// </summary>
	    /// <param name="xd">Current xmldocument context</param>
	    /// <returns>An xmlrepresentation of the macro</returns>
	    public XmlNode ToXml(XmlDocument xd)
	    {
            var serializer = new EntityXmlSerializer();
            var xml = serializer.Serialize(MacroEntity);
            return xml.GetXmlNode(xd);
	    }

	    [Obsolete("This does nothing")]
        public void RefreshProperties()
        {           
        }


		#region STATICS

		/// <summary>
		/// Creates a new macro given the name
		/// </summary>
		/// <param name="Name">Userfriendly name</param>
		/// <returns>The newly macro</returns>
		public static Macro MakeNew(string Name) 
		{
		    var macro = new Umbraco.Core.Models.Macro
		        {
                    Name = Name,
                    Alias = Name.Replace(" ", String.Empty)
		        };

		    ApplicationContext.Current.Services.MacroService.Save(macro);

            var newMacro = new Macro(macro);
           
            //fire new event
            var e = new NewEventArgs();
            newMacro.OnNew(e);
            
            return newMacro;
		}

		/// <summary>
		/// Retrieve all macroes
		/// </summary>
		/// <returns>A list of all macroes</returns>
		public static Macro[] GetAll()
		{
		    return ApplicationContext.Current.Services.MacroService.GetAll()
		                             .Select(x => new Macro(x))
		                             .ToArray();
		}

		/// <summary>
		/// Static contructor for retrieving a macro given an alias
		/// </summary>
        /// <param name="alias">The alias of the macro</param>
		/// <returns>If the macro with the given alias exists, it returns the macro, else null</returns>
        public static Macro GetByAlias(string alias)
		{
		    return ApplicationContext.Current.ApplicationCache.GetCacheItem(
		        GetCacheKey(alias),
		        TimeSpan.FromMinutes(30),
		        () =>
		            {
                        var macro = ApplicationContext.Current.Services.MacroService.GetByAlias(alias);
		                if (macro == null) return null;
		                return new Macro(macro);
		            });
		}

        public static Macro GetById(int id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                GetCacheKey(string.Format("macro_via_id_{0}", id)),
                TimeSpan.FromMinutes(30),
                () =>
                    {
                        var macro = ApplicationContext.Current.Services.MacroService.GetById(id);
                        if (macro == null) return null;
                        return new Macro(macro);
                    });
        }

        public static MacroTypes FindMacroType(string xslt, string scriptFile, string scriptType, string scriptAssembly)
        {
            if (string.IsNullOrEmpty(xslt) == false)
                return MacroTypes.XSLT;
	        
			if (string.IsNullOrEmpty(scriptFile) == false)
			{
				//we need to check if the file path saved is a virtual path starting with ~/Views/MacroPartials, if so then this is 
				//a partial view macro, not a script macro
				//we also check if the file exists in ~/App_Plugins/[Packagename]/Views/MacroPartials, if so then it is also a partial view.
				return (scriptFile.InvariantStartsWith(SystemDirectories.MvcViews + "/MacroPartials/")
				        || (Regex.IsMatch(scriptFile, "~/App_Plugins/.+?/Views/MacroPartials", RegexOptions.Compiled | RegexOptions.IgnoreCase)))
					       ? MacroTypes.PartialView
					       : MacroTypes.Script;
			}

	        if (string.IsNullOrEmpty(scriptType) == false && scriptType.InvariantContains(".ascx"))
		        return MacroTypes.UserControl;
	        
			if (string.IsNullOrEmpty(scriptType) == false && !string.IsNullOrEmpty(scriptAssembly))
		        return MacroTypes.CustomControl;

	        return MacroTypes.Unknown;
        }

        public static string GenerateCacheKeyFromCode(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException("input", "An MD5 hash cannot be generated when 'input' parameter is null!");

            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        #region Macro Refactor
        
        private static string GetCacheKey(string alias)
        {
            return CacheKeys.MacroCacheKey + alias;
        }

        #endregion


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
