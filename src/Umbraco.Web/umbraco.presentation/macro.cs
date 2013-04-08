using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Macros;
using Umbraco.Web.Templates;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.member;
using umbraco.DataLayer;
using umbraco.NodeFactory;
using umbraco.presentation.templateControls;
using Content = umbraco.cms.businesslogic.Content;
using Macro = umbraco.cms.businesslogic.macro.Macro;
using MacroErrorEventArgs = Umbraco.Core.Events.MacroErrorEventArgs;

namespace umbraco
{
    /// <summary>
    /// Summary description for macro.
    /// </summary>
    public class macro
    {
        #region private properties


        /// <summary>Cache for <see cref="GetPredefinedXsltExtensions"/>.</summary>
        private static Dictionary<string, object> m_PredefinedExtensions;

        private readonly string loadUserControlKey = "loadUserControl";

        private readonly StringBuilder mContent = new StringBuilder();
        private readonly Cache macroCache = HttpRuntime.Cache;

        private static readonly object macroRuntimeCacheSyncLock = new object();
        private static readonly string macroRuntimeCacheKey = "UmbracoRuntimeMacroCache";


        private readonly string macrosAddedKey = "macrosAdded";
        public IList<Exception> Exceptions = new List<Exception>();

        // Macro-elements


        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #endregion

        #region public properties

        public bool CacheByPersonalization
        {
            get { return Model.CacheByMember; }
        }

        public bool CacheByPage
        {
            get { return Model.CacheByPage; }
        }

        public bool DontRenderInEditor
        {
            get { return !Model.RenderInEditor; }
        }

        public int RefreshRate
        {
            get { return Model.CacheDuration; }
        }

        public String Alias
        {
            get { return Model.Alias; }
        }

        public String Name
        {
            get { return Model.Name; }
        }

        public String XsltFile
        {
            get { return Model.Xslt; }
        }

        public String ScriptFile
        {
            get { return Model.ScriptName; }
        }

        public String ScriptType
        {
            get { return Model.TypeName; }
        }

        public String ScriptAssembly
        {
            get { return Model.TypeName; }
        }

        public int MacroType
        {
            get { return (int)Model.MacroType; }
        }

        public String MacroContent
        {
            set { mContent.Append(value); }
            get { return mContent.ToString(); }
        }

        #endregion

        #region REFACTOR

        /// <summary>
        /// Creates a macro object
        /// </summary>
        /// <param name="id">Specify the macro-id which should be loaded (from table macro)</param>
        public macro(int id)
        {
            Macro m = Macro.GetById(id);
            Model = new MacroModel(m);
        }

        public macro(string alias)
        {
            Macro m = Macro.GetByAlias(alias);
            Model = new MacroModel(m);
        }

        public MacroModel Model { get; set; }

        public static macro GetMacro(string alias)
        {
            // FlorisRobbemont: issue #27610 -> Presentation macro not supposed to be cached.
            return new macro(alias);

            //return cms.businesslogic.cache.Cache.GetCacheItem(GetCacheKey(alias), macroRuntimeCacheSyncLock,
            //              TimeSpan.FromMinutes(60),
            //              delegate
            //              {
            //                  try
            //                  {
            //                      return new macro(alias);
            //                  }
            //                  catch
            //                  {
            //                      return null;
            //                  }
            //              });





        }







        public static macro GetMacro(int id)
        {
            // FlorisRobbemont: issue #27610 -> Presentation macro not supposed to be cached.






            return new macro(id);

            //return cms.businesslogic.cache.Cache.GetCacheItem(GetCacheKey(string.Format("by_id_{0}", id)), macroRuntimeCacheSyncLock,
            //              TimeSpan.FromMinutes(60),
            //              delegate
            //              {
            //                  try
            //                  {
            //                      return new macro(id);
            //                  }
            //                  catch
            //                  {
            //                      return null;
            //                  }
            //              });






        }

        #endregion

        private const string _xsltExtensionsCacheKey = "UmbracoXsltExtensions";

        private static readonly string _xsltExtensionsConfig =
            IOHelper.MapPath(SystemDirectories.Config + "/xsltExtensions.config");

        private static readonly object _xsltExtensionsSyncLock = new object();

        private static readonly Func<CacheDependency> _xsltExtensionsDependency =
            () => new CacheDependency(_xsltExtensionsConfig);

        /// <summary>
        /// Creates an empty macro object.
        /// </summary>
        public macro()
        {
            Model = new MacroModel();
        }


        public override string ToString()
        {
            return Model.Name;
        }

        private static string GetCacheKey(string alias)
        {
            return macroRuntimeCacheKey + alias;
        }

        /// <summary>
        /// Deletes macro definition from cache.
        /// </summary>
        /// <returns>True if succesfull, false if nothing has been removed</returns>
        //TODO: Update implementation!
        public bool removeFromCache()
        {
            if (Model.Id > 0)
            {
                cms.businesslogic.cache.Cache.ClearCacheItem(GetCacheKey(Model.Alias));
            }
            return false;
        }

        string GetCacheIdentifier(MacroModel model, Hashtable pageElements, int pageId)
        {
            StringBuilder id = new StringBuilder();

            var alias = string.IsNullOrEmpty(model.ScriptCode) ? model.Alias : Macro.GenerateCacheKeyFromCode(model.ScriptCode);
            id.AppendFormat("{0}-", alias);

            if (CacheByPage)
            {
                id.AppendFormat("{0}-", pageId);
            }

            if (CacheByPersonalization)
            {
                var currentMember = Member.GetCurrentMember();
                id.AppendFormat("m{0}-", currentMember == null ? 0 : currentMember.Id);
            }

            foreach (MacroPropertyModel prop in model.Properties)
            {
                var propValue = prop.Value;
                id.AppendFormat("{0}-", propValue.Length <= 255 ? propValue : propValue.Substring(0, 255));
            }

            return id.ToString();
        }

        public Control renderMacro(Hashtable attributes, Hashtable pageElements, int pageId)
        {
            // TODO: Parse attributes
            UpdateMacroModel(attributes);
            return renderMacro(pageElements, pageId);
        }

        public Control renderMacro(Hashtable pageElements, int pageId)
        {
	        TraceInfo("renderMacro",
	                  string.Format("Rendering started (macro: {0}, type: {1}, cacheRate: {2})",
	                                Name, MacroType, Model.CacheDuration));

            StateHelper.SetContextValue(macrosAddedKey, StateHelper.GetContextValue<int>(macrosAddedKey) + 1);

            String macroHtml = null;
            Control macroControl = null;

            // zb-00037 #29875 : parse attributes here (and before anything else)
            foreach (MacroPropertyModel prop in Model.Properties)
                prop.Value = helper.parseAttribute(pageElements, prop.Value);

            Model.CacheIdentifier = GetCacheIdentifier(Model, pageElements, pageId);

            if (!UmbracoContext.Current.InPreviewMode && Model.CacheDuration > 0)
            {
                if (cacheMacroAsString(Model))
                {
                    macroHtml = macroCache["macroHtml_" + Model.CacheIdentifier] as String;

                    // FlorisRobbemont: 
                    // An empty string means: macroHtml has been cached before, but didn't had any output (Macro doesn't need to be rendered again)
                    // An empty reference (null) means: macroHtml has NOT been cached before
                    if (macroHtml != null)
                    {
                        if (MacroNeedsToBeClearedFromCache(Model, "macroHtml_DateAdded_" + Model.CacheIdentifier))
                        {
                            macroHtml = null;
	                        TraceInfo("renderMacro",
	                                  string.Format("Macro removed from cache due to file change '{0}'.",
	                                                Model.CacheIdentifier));
                        }
                        else
                        {
	                        TraceInfo("renderMacro",
	                                  string.Format("Macro Content loaded from cache '{0}'.",
	                                                Model.CacheIdentifier));
                        }

                    }
                }
                else
                {
                    var cacheContent = macroCache["macroControl_" + Model.CacheIdentifier] as MacroCacheContent;

                    if (cacheContent != null)
                    {
                        macroControl = cacheContent.Content;
                        macroControl.ID = cacheContent.ID;

                        if (MacroNeedsToBeClearedFromCache(Model, "macroControl_DateAdded_" + Model.CacheIdentifier))
                        {
	                        TraceInfo("renderMacro",
	                                  string.Format("Macro removed from cache due to file change '{0}'.",
	                                                Model.CacheIdentifier));
                            macroControl = null;
                        }
                        else
                        {
	                        TraceInfo("renderMacro",
	                                  string.Format("Macro Control loaded from cache '{0}'.",
	                                                Model.CacheIdentifier));
                        }
    
                    }
                }
            }

            // FlorisRobbemont: Empty macroHtml (not null, but "") doesn't mean a re-render is necessary
            if (macroHtml == null && macroControl == null)
            {
                bool renderFailed = false;
                int macroType = Model.MacroType != MacroTypes.Unknown ? (int)Model.MacroType : MacroType;
                switch (macroType)
                {
					case (int)MacroTypes.PartialView:

                        //error handler for partial views, is an action because we need to re-use it twice below
                        Func<Exception, Control> handleError = e =>
                            {
                                LogHelper.WarnWithException<macro>("Error loading Partial View (file: " + ScriptFile + ")", true, e);
                                // Invoke any error handlers for this macro
                                var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = Model.ScriptName, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour };
                                return GetControlForErrorBehavior("Error loading Partial View script (file: " + ScriptFile + ")", macroErrorEventArgs);
                            };

						TraceInfo("umbracoMacro", "Partial View added (" + Model.TypeName + ")");
						try
						{							
							var result = LoadPartialViewMacro(Model);
							macroControl = new LiteralControl(result.Result);
							if (result.ResultException != null)
							{
								renderFailed = true;
                                Exceptions.Add(result.ResultException);
                                macroControl = handleError(result.ResultException);
                                //if it is null, then we are supposed to throw the exception
                                if (macroControl == null)
                                {
                                    throw result.ResultException;
                                }
							}
						}
						catch (Exception e)
						{
							renderFailed = true;
							Exceptions.Add(e);						    
						    macroControl = handleError(e);
                            //if it is null, then we are supposed to throw the (original) exception
                            // see: http://issues.umbraco.org/issue/U4-497 at the end
                            if (macroControl == null)
                            {
                                throw;
                            }
						}

		                break;
                    case (int)MacroTypes.UserControl:
                        try
                        {
							TraceInfo("umbracoMacro","Usercontrol added (" + Model.TypeName + ")");
                            
                            // Add tilde for v4 defined macros
                            if (string.IsNullOrEmpty(Model.TypeName) == false && Model.TypeName.StartsWith("~") == false)
                                Model.TypeName = "~/" + Model.TypeName;

                            macroControl = loadUserControl(ScriptType, Model, pageElements);
                            break;
                        }
                        catch (Exception e)
                        {
                            renderFailed = true;
                            Exceptions.Add(e);
							LogHelper.WarnWithException<macro>("Error loading userControl (" + Model.TypeName + ")", true, e);

                            // Invoke any error handlers for this macro
                            var macroErrorEventArgs = new MacroErrorEventArgs {Name = Model.Name, Alias = Model.Alias, ItemKey = Model.TypeName, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour};

                            macroControl = GetControlForErrorBehavior("Error loading userControl '" + Model.TypeName + "'", macroErrorEventArgs);
                            //if it is null, then we are supposed to throw the (original) exception
                            // see: http://issues.umbraco.org/issue/U4-497 at the end
                            if (macroControl == null)
                            {
                                throw;
                            }

                            break;
                        }
                    case (int)MacroTypes.CustomControl:
                        try
                        {
	                        TraceInfo("umbracoMacro", "Custom control added (" + Model.TypeName + ")");
	                        TraceInfo("umbracoMacro", "ScriptAssembly (" + Model.TypeAssembly + ")");
                            macroControl = loadControl(Model.TypeAssembly, ScriptType, Model, pageElements);
                            break;
                        }
                        catch (Exception e)
                        {
                            renderFailed = true;
                            Exceptions.Add(e);

	                        LogHelper.WarnWithException<macro>("Error loading customControl (Assembly: " + Model.TypeAssembly + ", Type: '" + Model.TypeName + "'", true, e);

                            // Invoke any error handlers for this macro
                            var macroErrorEventArgs = new MacroErrorEventArgs {Name = Model.Name, Alias = Model.Alias, ItemKey = Model.TypeAssembly, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour};

                            macroControl = GetControlForErrorBehavior("Error loading customControl (Assembly: " + Model.TypeAssembly + ", Type: '" + Model.TypeName + "'", macroErrorEventArgs);
                            //if it is null, then we are supposed to throw the (original) exception
                            // see: http://issues.umbraco.org/issue/U4-497 at the end
                            if (macroControl == null)
                            {
                                throw;
                            }

                            break;
                        }
                    case (int)MacroTypes.XSLT:
                        macroControl = LoadMacroXslt(this, Model, pageElements, true);
                        break;
                    case (int)MacroTypes.Script:

                        //error handler for partial views, is an action because we need to re-use it twice below
                        Func<Exception, Control> handleMacroScriptError = e =>
                        {
                            LogHelper.WarnWithException<macro>("Error loading MacroEngine script (file: " + ScriptFile + ", Type: '" + Model.TypeName + "'", true, e);

                            // Invoke any error handlers for this macro
                            var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = ScriptFile, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour };

                            return GetControlForErrorBehavior("Error loading MacroEngine script (file: " + ScriptFile + ")", macroErrorEventArgs);
                        };
                        
                        try
                        {
	                        TraceInfo("umbracoMacro", "MacroEngine script added (" + ScriptFile + ")");
                            ScriptingMacroResult result = loadMacroScript(Model);
                            macroControl = new LiteralControl(result.Result);
                            if (result.ResultException != null)
                            {
                                renderFailed = true;
                                Exceptions.Add(result.ResultException);
                                macroControl = handleMacroScriptError(result.ResultException);
                                //if it is null, then we are supposed to throw the exception
                                if (macroControl == null)
                                {
                                    throw result.ResultException;
                                }
                            }
                            break;
                        }
                        catch (Exception e)
                        {
                            renderFailed = true;
                            Exceptions.Add(e);

                            macroControl = handleMacroScriptError(e);
                            //if it is null, then we are supposed to throw the (original) exception
                            // see: http://issues.umbraco.org/issue/U4-497 at the end
                            if (macroControl == null)
                            {
                                throw;
                            }

                            break;
                        }
                    default:
                        if (GlobalSettings.DebugMode)
                            macroControl = new LiteralControl("&lt;Macro: " + Name + " (" + ScriptAssembly + "," + ScriptType + ")&gt;");
                        break;
                }

                // Add result to cache if successful
                if (!renderFailed && Model.CacheDuration > 0)
                {
                    // do not add to cache if there's no member and it should cache by personalization
                    if (!Model.CacheByMember || (Model.CacheByMember && Member.GetCurrentMember() != null))
                    {
                        if (macroControl != null)
                        {
                            // NH: Scripts and XSLT can be generated as strings, but not controls as page events wouldn't be hit (such as Page_Load, etc)
                            if (cacheMacroAsString(Model))
                            {
                                string outputCacheString = "";

                                using (var sw = new StringWriter())
                                {
                                    var hw = new HtmlTextWriter(sw);
                                    macroControl.RenderControl(hw);

                                    outputCacheString = sw.ToString();
                                }

                                macroCache.Insert("macroHtml_" + Model.CacheIdentifier,
                                                outputCacheString,

                                                null,
                                                DateTime.Now.AddSeconds(Model.CacheDuration),
                                                TimeSpan.Zero,
                                                CacheItemPriority.NotRemovable, //FlorisRobbemont: issue #27610 -> Macro output cache should not be removable

                                                null);



                                                macroCache.Insert("macroHtml_DateAdded_" + Model.CacheIdentifier,
                                                DateTime.Now,

                                                null,
                                                DateTime.Now.AddSeconds(Model.CacheDuration),
                                                TimeSpan.Zero,
                                                CacheItemPriority.NotRemovable, //FlorisRobbemont: issue #27610 -> Macro output cache should not be removable

                                                null);

                                // zb-00003 #29470 : replace by text if not already text
                                // otherwise it is rendered twice
                                if (!(macroControl is LiteralControl))
                                    macroControl = new LiteralControl(outputCacheString);

	                            TraceInfo("renderMacro",
	                                      string.Format("Macro Content saved to cache '{0}'.", Model.CacheIdentifier));

                            }

                            else
                            {
                                macroCache.Insert("macroControl_" + Model.CacheIdentifier,
                                                new MacroCacheContent(macroControl, macroControl.ID), null,
                                                DateTime.Now.AddSeconds(Model.CacheDuration), TimeSpan.Zero,
                                                CacheItemPriority.NotRemovable, //FlorisRobbemont: issue #27610 -> Macro output cache should not be removable

                                                null);


                                macroCache.Insert("macroControl_DateAdded_" + Model.CacheIdentifier,
                                                 DateTime.Now, null,
                                                 DateTime.Now.AddSeconds(Model.CacheDuration), TimeSpan.Zero,
                                                 CacheItemPriority.NotRemovable, //FlorisRobbemont: issue #27610 -> Macro output cache should not be removable

                                                 null);

	                            TraceInfo("renderMacro",
	                                      string.Format("Macro Control saved to cache '{0}'.", Model.CacheIdentifier));
                            }
                        }
                    }
                }
            }
            else if (macroControl == null)
            {
                // FlorisRobbemont: Extra check to see if the macroHtml is not null
                macroControl = new LiteralControl(macroHtml == null ? "" : macroHtml);
            }

            return macroControl;
        }

        /// <summary>
        /// Raises the error event and based on the error behavior either return a control to display or throw the exception
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Control GetControlForErrorBehavior(string msg, MacroErrorEventArgs args)
        {
            OnError(args);

            switch (args.Behaviour)
            {
                case MacroErrorBehaviour.Inline:
                    return new LiteralControl(msg);
                case MacroErrorBehaviour.Silent:
                    return new LiteralControl("");
                case MacroErrorBehaviour.Throw:
                default:
                    return null;
            }
        }

        /// <summary>
        /// check that the file has not recently changed
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        private bool MacroNeedsToBeClearedFromCache(MacroModel Model,string dateAddedKey)
        {
            
            if(MacroIsFileBased(Model))
            {
                if(macroCache[dateAddedKey]!=null)
                {
                    DateTime dateMacroAdded = DateTime.Parse(macroCache[dateAddedKey].ToString());

                    string macroFile = GetMacroFile(Model);
                    FileInfo fileInfo = new FileInfo(HttpContext.Current.Server.MapPath(macroFile));


                    if (fileInfo.LastWriteTime.CompareTo(dateMacroAdded) ==1)
                    {
	                    TraceInfo("renderMacro",
	                              string.Format("Macro needs to be removed from cache due to file change '{0}'.", Model.CacheIdentifier));
                        return true;
                    }
                  
                }

            }

            return false;
        }

        private string GetMacroFile(MacroModel Model)
        {
           if(Model.Xslt!=string.Empty)
           {
               return string.Concat("/xslt/", Model.Xslt);
           }
           if(Model.ScriptName!=string.Empty)
           {
               return string.Concat("/macroScripts/" + Model.ScriptName);
           }
            //??
            return "/" + Model.TypeName;
        }

        private static bool MacroIsFileBased(MacroModel Model)
        {
            return Model.MacroType!=MacroTypes.CustomControl;
        }

        private bool cacheMacroAsString(MacroModel model)
        {
            //FlorisRobbemont: issue #27610 -> Changed this to include Razor scripts files as String caching 
            return model.MacroType == MacroTypes.XSLT || model.MacroType == MacroTypes.Python || model.MacroType == MacroTypes.Script;
        }

        public static XslCompiledTransform getXslt(string XsltFile)
        {
            if (HttpRuntime.Cache["macroXslt_" + XsltFile] != null)
            {
                return (XslCompiledTransform)HttpRuntime.Cache["macroXslt_" + XsltFile];
            }
            else
            {
                var xslReader =
                    new XmlTextReader(IOHelper.MapPath(SystemDirectories.Xslt + "/" + XsltFile));

                XslCompiledTransform macroXSLT = CreateXsltTransform(xslReader, GlobalSettings.DebugMode);
                HttpRuntime.Cache.Insert(
                    "macroXslt_" + XsltFile,
                    macroXSLT,
                    new CacheDependency(IOHelper.MapPath(SystemDirectories.Xslt + "/" + XsltFile)));
                return macroXSLT;
            }
        }

        public void UpdateMacroModel(Hashtable attributes)
        {
            foreach (MacroPropertyModel mp in Model.Properties)
            {
                if (attributes.ContainsKey(mp.Key.ToLower()))
                {
                    mp.Value = attributes[mp.Key.ToLower()].ToString();
                }
                else
                {
                    mp.Value = string.Empty;
                }
            }
        }

        public void GenerateMacroModelPropertiesFromAttributes(Hashtable attributes)
        {
            foreach (string key in attributes.Keys)
            {
                Model.Properties.Add(new MacroPropertyModel(key, attributes[key].ToString()));
            }
        }


        public static XslCompiledTransform CreateXsltTransform(XmlTextReader xslReader, bool debugMode)
        {
            var macroXSLT = new XslCompiledTransform(debugMode);
            var xslResolver = new XmlUrlResolver();
            xslResolver.Credentials = CredentialCache.DefaultCredentials;

            xslReader.EntityHandling = EntityHandling.ExpandEntities;

            try
            {
                if (GlobalSettings.ApplicationTrustLevel > AspNetHostingPermissionLevel.Medium)
                {
                    macroXSLT.Load(xslReader, XsltSettings.TrustedXslt, xslResolver);
                }
                else
                {
                    macroXSLT.Load(xslReader, XsltSettings.Default, xslResolver);
                }
            }
            finally
            {
                xslReader.Close();
            }

            return macroXSLT;
        }

        public static void unloadXslt(string XsltFile)
        {
            if (HttpRuntime.Cache["macroXslt_" + XsltFile] != null)
                HttpRuntime.Cache.Remove("macroXslt_" + XsltFile);
        }

        internal Control LoadMacroXslt(macro macro, MacroModel model, Hashtable pageElements, bool throwError)
        {
            if (XsltFile.Trim() != string.Empty)
            {
                // Get main XML
                var umbracoXml = content.Instance.XmlContent;

                // Create XML document for Macro
                var macroXml = new XmlDocument();
                macroXml.LoadXml("<macro/>");

                foreach (var prop in macro.Model.Properties)
                {
                    addMacroXmlNode(umbracoXml, macroXml, prop.Key, prop.Type, prop.Value);
                }

                if (HttpContext.Current.Request.QueryString["umbDebug"] != null && GlobalSettings.DebugMode)
                {
                    return
                        new LiteralControl("<div style=\"border: 2px solid green; padding: 5px;\"><b>Debug from " +
                                           macro.Name +
                                           "</b><br/><p>" + HttpContext.Current.Server.HtmlEncode(macroXml.OuterXml) +
                                           "</p></div>");
                }

                try
                {
                    var xsltFile = getXslt(XsltFile);

                    try
                    {
                        var result = CreateControlsFromText(GetXsltTransformResult(macroXml, xsltFile));

                        TraceInfo("umbracoMacro", "After performing transformation");

                        return result;
                    }
                    catch (Exception e)
                    {
                        Exceptions.Add(e);
                        LogHelper.WarnWithException<macro>("Error parsing XSLT file", e);
                        // inner exception code by Daniel Lindstr?m from SBBS.se
                        Exception ie = e;
                        while (ie != null)
                        {
                            TraceWarn("umbracoMacro InnerException", ie.Message, ie);
                            ie = ie.InnerException;
                        }

                        var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = Model.Xslt, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour };
                        var macroControl = GetControlForErrorBehavior("Error parsing XSLT file: \\xslt\\" + XsltFile, macroErrorEventArgs);
                        //if it is null, then we are supposed to throw the (original) exception
                        // see: http://issues.umbraco.org/issue/U4-497 at the end
                        if (macroControl == null && throwError)
                        {
                            throw;
                        }
                        return macroControl;
                    }
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    LogHelper.WarnWithException<macro>("Error loading XSLT " + Model.Xslt, true, e);

                    // Invoke any error handlers for this macro
                    var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = Model.Xslt, Exception = e, Behaviour = UmbracoSettings.MacroErrorBehaviour };
                    var macroControl = GetControlForErrorBehavior("Error reading XSLT file: \\xslt\\" + XsltFile, macroErrorEventArgs);
                    //if it is null, then we are supposed to throw the (original) exception
                    // see: http://issues.umbraco.org/issue/U4-497 at the end
                    if (macroControl == null && throwError)
                    {
                        throw;
                    }
                    return macroControl;
                }
            }

            TraceWarn("macro", "Xslt is empty");
            return new LiteralControl(string.Empty);
        }

        public Control loadMacroXSLT(macro macro, MacroModel model, Hashtable pageElements)
        {
            return LoadMacroXslt(macro, model, pageElements, false);
        }

        /// <summary>
        /// Parses the text for umbraco Item controls that need to be rendered.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>A control containing the parsed text.</returns>
        protected Control CreateControlsFromText(string text)
        {
            // the beginning and end tags
            const string tagStart = "[[[[umbraco:Item";
            const string tagEnd = "]]]]";

            // container that will hold parsed controls
            var container = new PlaceHolder();

            // loop through all text
            int textPos = 0;
            while (textPos < text.Length)
            {
                // try to find an item tag, carefully staying inside the string bounds (- 1)
                int tagStartPos = text.IndexOf(tagStart, textPos);
                int tagEndPos = tagStartPos < 0 ? -1 : text.IndexOf(tagEnd, tagStartPos + tagStart.Length - 1);

                // item tag found?
                if (tagStartPos >= 0 && tagEndPos >= 0)
                {
                    // add the preceding text as a literal control
                    if (tagStartPos > textPos)
                        container.Controls.Add(new LiteralControl(text.Substring(textPos, tagStartPos - textPos)));

                    // extract the tag and parse it
                    string tag = text.Substring(tagStartPos, (tagEndPos + tagEnd.Length) - tagStartPos);
                    Hashtable attributes = helper.ReturnAttributes(tag);

                    // create item with the parameters specified in the tag
                    var item = new Item();
                    item.NodeId = helper.FindAttribute(attributes, "nodeid");
                    item.Field = helper.FindAttribute(attributes, "field");
                    item.Xslt = helper.FindAttribute(attributes, "xslt");
                    item.XsltDisableEscaping = helper.FindAttribute(attributes, "xsltdisableescaping") == "true";
                    container.Controls.Add(item);

                    // advance past the end of the tag
                    textPos = tagEndPos + tagEnd.Length;
                }
                else
                {
                    // no more tags found, just add the remaning text
                    container.Controls.Add(new LiteralControl(text.Substring(textPos)));
                    textPos = text.Length;
                }
            }
            return container;
        }

        public static string GetXsltTransformResult(XmlDocument macroXML, XslCompiledTransform xslt)
        {
            return GetXsltTransformResult(macroXML, xslt, null);
        }

        public static string GetXsltTransformResult(XmlDocument macroXML, XslCompiledTransform xslt,
                                                    Dictionary<string, object> parameters)
        {
            TextWriter tw = new StringWriter();

            TraceInfo("umbracoMacro", "Before adding extensions");
            XsltArgumentList xslArgs;
            xslArgs = AddXsltExtensions();
            var lib = new library();
            xslArgs.AddExtensionObject("urn:umbraco.library", lib);
			TraceInfo("umbracoMacro", "After adding extensions");

            // Add parameters
            if (parameters == null || !parameters.ContainsKey("currentPage"))
            {
                xslArgs.AddParam("currentPage", string.Empty, library.GetXmlNodeCurrent());
            }
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                    xslArgs.AddParam(parameter.Key, string.Empty, parameter.Value);
            }

            // Do transformation
			TraceInfo("umbracoMacro", "Before performing transformation");
            xslt.Transform(macroXML.CreateNavigator(), xslArgs, tw);
			return TemplateUtilities.ResolveUrlsFromTextString(tw.ToString());
        }

        public static XsltArgumentList AddXsltExtensions()
        {
            return AddMacroXsltExtensions();
        }

        /// <summary>
        /// Gets a collection of all XSLT extensions for macros, including predefined extensions.
        /// </summary>
        /// <returns>A dictionary of name/extension instance pairs.</returns>
        public static Dictionary<string, object> GetXsltExtensions()
        {
            // zb-00041 #29966 : cache the extensions

            // We could cache the extensions in a static variable but then the cache
            // would not be refreshed when the .config file is modified. An application
            // restart would be required. Better use the cache and add a dependency.
			
			// SD: Not sure what is meant by the above statement? Having these in a static variable would be preferred!
			//  If you modify a config file, the app restarts and thus all static variables are reset.
			//  Having this stuff in cache just adds to the gigantic amount of cache data and will cause more cache turnover to happen.

            return cms.businesslogic.cache.Cache.GetCacheItem(
                _xsltExtensionsCacheKey, _xsltExtensionsSyncLock,
                CacheItemPriority.NotRemovable, // NH 4.7.1, Changing to NotRemovable
                null, // no refresh action
                _xsltExtensionsDependency(), // depends on the .config file
                TimeSpan.FromDays(1), // expires in 1 day (?)
                GetXsltExtensionsImpl);
        }

        // zb-00041 #29966 : cache the extensions


        private static Dictionary<string, object> GetXsltExtensionsImpl()
        {
            // fill a dictionary with the predefined extensions
            var extensions = new Dictionary<string, object>(GetPredefinedXsltExtensions());

            // Load the XSLT extensions configuration
            var xsltExt = new XmlDocument();
            xsltExt.Load(_xsltExtensionsConfig);

            // add all descendants of the XsltExtensions element
            foreach (XmlNode xsltEx in xsltExt.SelectSingleNode("/XsltExtensions"))
            {
                if (xsltEx.NodeType == XmlNodeType.Element)
                {
                    Debug.Assert(xsltEx.Attributes["assembly"] != null, "Extension attribute 'assembly' not specified.");
                    Debug.Assert(xsltEx.Attributes["type"] != null, "Extension attribute 'type' not specified.");
                    Debug.Assert(xsltEx.Attributes["alias"] != null, "Extension attribute 'alias' not specified.");

                    // load the extension assembly
                    string extensionFile =
                        IOHelper.MapPath(string.Format("{0}/{1}.dll", SystemDirectories.Bin,
                                                       xsltEx.Attributes["assembly"].Value));

                    Assembly extensionAssembly;
                    try
                    {
                        extensionAssembly = Assembly.LoadFrom(extensionFile);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            String.Format(
                                "Could not load assembly {0} for XSLT extension {1}. Please check config/xsltExtensions.config.",
                                extensionFile, xsltEx.Attributes["alias"].Value), ex);
                    }

                    // load the extension type
                    Type extensionType = extensionAssembly.GetType(xsltEx.Attributes["type"].Value);
                    if (extensionType == null)
                        throw new Exception(
                            String.Format(
                                "Could not load type {0} ({1}) for XSLT extension {1}. Please check config/xsltExtensions.config.",
                                xsltEx.Attributes["type"].Value, extensionFile, xsltEx.Attributes["alias"].Value));

                    // create an instance and add it to the extensions list
                    extensions.Add(xsltEx.Attributes["alias"].Value, Activator.CreateInstance(extensionType));
                }
            }

            //also get types marked with XsltExtension attribute

            // zb-00042 #29949 : do not hide errors, refactor
        	
			var foundExtensions = Umbraco.Web.PluginManagerExtensions.ResolveXsltExtensions(PluginManager.Current);
			foreach (var xsltType in foundExtensions)
            {
                var tpAttributes = xsltType.GetCustomAttributes(typeof(XsltExtensionAttribute), true);
                foreach (XsltExtensionAttribute tpAttribute in tpAttributes)
                {
                    var ns = !string.IsNullOrEmpty(tpAttribute.Namespace) 
						? tpAttribute.Namespace 
						: xsltType.FullName;
                    extensions.Add(ns, Activator.CreateInstance(xsltType));
                }
            }

            return extensions;
        }

        /// <summary>
        /// Gets the predefined XSLT extensions.
        /// </summary>
        /// <remarks>
        ///     This is a legacy list of EXSLT extensions.
        ///     The Umbraco library is not included, because its instance is page specific.
        /// </remarks>
        /// <returns>A dictionary of name/extension instance pairs.</returns>
        public static Dictionary<string, object> GetPredefinedXsltExtensions()
        {
            if (m_PredefinedExtensions == null)
            {
                m_PredefinedExtensions = new Dictionary<string, object>();

                // [LK] U4-86 Move EXSLT references from being predefined in core to xsltExtensions.config
                //// add predefined EXSLT extensions
                //m_PredefinedExtensions.Add("Exslt.ExsltCommon", new ExsltCommon());
                //m_PredefinedExtensions.Add("Exslt.ExsltDatesAndTimes", new ExsltDatesAndTimes());
                //m_PredefinedExtensions.Add("Exslt.ExsltMath", new ExsltMath());
                //m_PredefinedExtensions.Add("Exslt.ExsltRegularExpressions", new ExsltRegularExpressions());
                //m_PredefinedExtensions.Add("Exslt.ExsltStrings", new ExsltStrings());
                //m_PredefinedExtensions.Add("Exslt.ExsltSets", new ExsltSets());
            }

            return m_PredefinedExtensions;
        }

        /// <summary>
        /// Returns an XSLT argument list with all XSLT extensions added,
        /// both predefined and configured ones.
        /// </summary>
        /// <returns>A new XSLT argument list.</returns>
        public static XsltArgumentList AddMacroXsltExtensions()
        {
            var xslArgs = new XsltArgumentList();

            foreach (var extension in GetXsltExtensions())
            {
                string extensionNamespace = "urn:" + extension.Key;
                xslArgs.AddExtensionObject(extensionNamespace, extension.Value);
	            TraceInfo("umbracoXsltExtension",
	                      String.Format("Extension added: {0}, {1}",
	                                    extensionNamespace, extension.Value.GetType().Name));
            }

            return xslArgs;
        }

        private void addMacroXmlNode(XmlDocument umbracoXML, XmlDocument macroXML, String macroPropertyAlias,
                                     String macroPropertyType, String macroPropertyValue)
        {
            XmlNode macroXmlNode = macroXML.CreateNode(XmlNodeType.Element, macroPropertyAlias, string.Empty);
            var x = new XmlDocument();

            int currentID = -1;
            // If no value is passed, then use the current pageID as value
            if (macroPropertyValue == string.Empty)
            {
                var umbPage = (page)HttpContext.Current.Items["umbPageObject"];
                if (umbPage == null)
                    return;
                currentID = umbPage.PageID;
            }

	        TraceInfo("umbracoMacro",
	                  "Xslt node adding search start (" + macroPropertyAlias + ",'" +
	                  macroPropertyValue + "')");
            switch (macroPropertyType)
            {
                case "contentTree":
                    XmlAttribute nodeID = macroXML.CreateAttribute("nodeID");
                    if (macroPropertyValue != string.Empty)
                        nodeID.Value = macroPropertyValue;
                    else
                        nodeID.Value = currentID.ToString();
                    macroXmlNode.Attributes.SetNamedItem(nodeID);

                    // Get subs
                    try
                    {
                        macroXmlNode.AppendChild(macroXML.ImportNode(umbracoXML.GetElementById(nodeID.Value), true));
                    }
                    catch
                    {
                        break;
                    }
                    break;
                case "contentCurrent":
                    x.LoadXml("<nodes/>");
                    XmlNode currentNode;
                    if (macroPropertyValue != string.Empty)
                        currentNode = macroXML.ImportNode(umbracoXML.GetElementById(macroPropertyValue), true);
                    else
                        currentNode = macroXML.ImportNode(umbracoXML.GetElementById(currentID.ToString()), true);

                    // remove all sub content nodes
                    foreach (XmlNode n in currentNode.SelectNodes("node|*[@isDoc]"))
                        currentNode.RemoveChild(n);

                    macroXmlNode.AppendChild(currentNode);

                    break;
                case "contentSubs":
                    x.LoadXml("<nodes/>");
                    if (macroPropertyValue != string.Empty)
                        x.FirstChild.AppendChild(x.ImportNode(umbracoXML.GetElementById(macroPropertyValue), true));
                    else
                        x.FirstChild.AppendChild(x.ImportNode(umbracoXML.GetElementById(currentID.ToString()), true));
                    macroXmlNode.InnerXml = transformMacroXML(x, "macroGetSubs.xsl");
                    break;
                case "contentAll":
                    x.ImportNode(umbracoXML.DocumentElement.LastChild, true);
                    break;
                case "contentRandom":
                    XmlNode source = umbracoXML.GetElementById(macroPropertyValue);
					if (source != null)
					{
						XmlNodeList sourceList = source.SelectNodes("node|*[@isDoc]");
						if (sourceList.Count > 0)
						{
							int rndNumber;
							Random r = library.GetRandom();
							lock (r)
							{
								rndNumber = r.Next(sourceList.Count);
							}
							XmlNode node = macroXML.ImportNode(sourceList[rndNumber], true);
							// remove all sub content nodes
							foreach (XmlNode n in node.SelectNodes("node|*[@isDoc]"))
								node.RemoveChild(n);

							macroXmlNode.AppendChild(node);
							break;
						}
						else
							TraceWarn("umbracoMacro",
									  "Error adding random node - parent (" + macroPropertyValue +
									  ") doesn't have children!");
					}
					else
						TraceWarn("umbracoMacro",
						          "Error adding random node - parent (" + macroPropertyValue +
						          ") doesn't exists!");
                    break;
                case "mediaCurrent":
                    var c = new Content(int.Parse(macroPropertyValue));
                    macroXmlNode.AppendChild(macroXML.ImportNode(c.ToXml(content.Instance.XmlContent, false), true));
                    break;
                default:
                    macroXmlNode.InnerText = HttpContext.Current.Server.HtmlDecode(macroPropertyValue);
                    break;
            }
            macroXML.FirstChild.AppendChild(macroXmlNode);
        }

        private string transformMacroXML(XmlDocument xmlSource, string xslt_File)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            XslCompiledTransform result = getXslt(xslt_File);
            //XmlDocument xslDoc = new XmlDocument();

            result.Transform(xmlSource.CreateNavigator(), null, sw);

            if (sw.ToString() != string.Empty)
                return sw.ToString();
            else
                return string.Empty;
        }

		/// <summary>
		/// Renders a Partial View Macro
		/// </summary>
		/// <param name="macro"></param>
		/// <returns></returns>
		internal ScriptingMacroResult LoadPartialViewMacro(MacroModel macro)
		{
			var retVal = new ScriptingMacroResult();
			TraceInfo("umbracoMacro", "Rendering Partial View Macro");
			IMacroEngine engine = null;
			
			engine = MacroEngineFactory.GetEngine(PartialViewMacroEngine.EngineName);
			var ret = engine.Execute(macro, Node.GetCurrent());

			// if the macro engine supports success reporting and executing failed, then return an empty control so it's not cached
			if (engine is IMacroEngineResultStatus)
			{
				var result = engine as IMacroEngineResultStatus;
				if (!result.Success)
				{
					retVal.ResultException = result.ResultException;
				}
			}
			TraceInfo("umbracoMacro", "Rendering Partial View Macro [done]");
			retVal.Result = ret;
			return retVal;
		}

        public ScriptingMacroResult loadMacroScript(MacroModel macro)
        {
            var retVal = new ScriptingMacroResult();
            TraceInfo("umbracoMacro", "Loading IMacroEngine script");
            string ret = String.Empty;
            IMacroEngine engine = null;
            if (!String.IsNullOrEmpty(macro.ScriptCode))
            {
                engine = MacroEngineFactory.GetByExtension(macro.ScriptLanguage);
                ret = engine.Execute(
                    macro,
                    Node.GetCurrent());
            }
            else
            {
                string path = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + macro.ScriptName);
                engine = MacroEngineFactory.GetByFilename(path);
                ret = engine.Execute(macro, Node.GetCurrent());
            }

            // if the macro engine supports success reporting and executing failed, then return an empty control so it's not cached
            if (engine is IMacroEngineResultStatus)
            {
                var result = engine as IMacroEngineResultStatus;
                if (!result.Success)
                {
                    retVal.ResultException = result.ResultException;
                }
            }
            TraceInfo("umbracoMacro", "Loading IMacroEngine script [done]");
            retVal.Result = ret;
            return retVal;
        }



        [Obsolete("Replaced with loadMacroScript", true)]
        public DLRMacroResult loadMacroDLR(MacroModel macro)
        {
            var retVal = new DLRMacroResult();
            TraceInfo("umbracoMacro", "Loading IMacroEngine script");
            var ret = new LiteralControl();
            IMacroEngine engine = null;
            if (!String.IsNullOrEmpty(macro.ScriptCode))
            {
                engine = MacroEngineFactory.GetByExtension(macro.ScriptLanguage);
                ret.Text = engine.Execute(
                    macro,
                    Node.GetCurrent());
            }
            else
            {
                string path = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + macro.ScriptName);
                engine = MacroEngineFactory.GetByFilename(path);
                ret.Text = engine.Execute(macro, Node.GetCurrent());
            }

            // if the macro engine supports success reporting and executing failed, then return an empty control so it's not cached
            if (engine is IMacroEngineResultStatus)
            {
                var result = engine as IMacroEngineResultStatus;
                if (!result.Success)
                {
                    retVal.ResultException = result.ResultException;
                }
            }
            TraceInfo("umbracoMacro", "Loading IMacroEngine script [done]");
            retVal.Control = ret;
            return retVal;
        }

        /// <summary>
        /// Loads a custom or webcontrol using reflection into the macro object
        /// </summary>
        /// <param name="fileName">The assembly to load from</param>
        /// <param name="controlName">Name of the control</param>
        /// <returns></returns>
        /// <param name="attributes"></param>
        public Control loadControl(string fileName, string controlName, MacroModel model)
        {
            return loadControl(fileName, controlName, model, null);
        }

        /// <summary>
        /// Loads a custom or webcontrol using reflection into the macro object
        /// </summary>
        /// <param name="fileName">The assembly to load from</param>
        /// <param name="controlName">Name of the control</param>
        /// <returns></returns>
        /// <param name="attributes"></param>
        /// <param name="umbPage"></param>
        public Control loadControl(string fileName, string controlName, MacroModel model, Hashtable pageElements)
        {
            Type type;
            Assembly asm;
            try
            {
                string currentAss = IOHelper.MapPath(string.Format("{0}/{1}.dll", SystemDirectories.Bin, fileName));

                if (!File.Exists(currentAss))
                    return new LiteralControl("Unable to load user control because is does not exist: " + fileName);
                asm = Assembly.LoadFrom(currentAss);
                
				TraceInfo("umbracoMacro", "Assembly file " + currentAss + " LOADED!!");
            }
            catch
            {
                throw new ArgumentException(string.Format("ASSEMBLY NOT LOADED PATH: {0} NOT FOUND!!",
                                                          IOHelper.MapPath(SystemDirectories.Bin + "/" + fileName +
                                                                           ".dll")));
            }

	        TraceInfo("umbracoMacro", string.Format("Assembly Loaded from ({0}.dll)", fileName));
            type = asm.GetType(controlName);
            if (type == null)
                return new LiteralControl(string.Format("Unable to get type {0} from assembly {1}",
                                                        controlName, asm.FullName));

            var control = Activator.CreateInstance(type) as Control;
            if (control == null)
                return new LiteralControl(string.Format("Unable to create control {0} from assembly {1}",
                                                        controlName, asm.FullName));

            AddCurrentNodeToControl(control, type);

            // Properties
            updateControlProperties(type, control, model);
            return control;
        }

        private void updateControlProperties(Type type, Control control, MacroModel model)
        {
            foreach (MacroPropertyModel mp in model.Properties)
            {
                PropertyInfo prop = type.GetProperty(mp.Key);
                if (prop == null)
                {					
					TraceWarn("macro", string.Format("control property '{0}' doesn't exist or aren't accessible (public)", mp.Key));
                    continue;
                }

                object propValue = mp.Value;
                bool propValueSet = false;
                // Special case for types of webControls.unit
                if (prop.PropertyType == typeof(Unit))
                {
                    propValue = Unit.Parse(propValue.ToString());
                    propValueSet = true;
                }
                else
                {
                    try
                    {
                        if (mp.CLRType == null)
                            continue;
                        var st = (TypeCode)Enum.Parse(typeof(TypeCode), mp.CLRType, true);

                        // Special case for booleans
                        if (prop.PropertyType == typeof(bool))
                        {
                            bool parseResult;
                            if (
                                Boolean.TryParse(
                                    propValue.ToString().Replace("1", "true").Replace("0", "false"),
                                    out parseResult))
                                propValue = parseResult;
                            else
                                propValue = false;
                            propValueSet = true;

                        }
                        else
                        {
                            string sPropValue = string.Format("{0}", propValue);
                            Type propType = prop.PropertyType;
                            if (!string.IsNullOrWhiteSpace(sPropValue))
                            {
                                try
                                {
                                    propValue = Convert.ChangeType(propValue, st);
                                    propValueSet = true;
                                }
                                catch (FormatException)
                                {
                                    propValue = Convert.ChangeType(propValue, propType);
                                    propValueSet = true;
                                }
                            }
                            /* NH 06-01-2012: Remove the lines below as they would only get activated if the values are empty
                        else
                        {
                            if (propType != null)
                            {
                                if (propType.IsValueType)
                                {
                                    propValue = Activator.CreateInstance(propType);
                                }
                                else
                                {
                                    propValue = null;
                                }
                            }
                        }*/
                        }

						if (GlobalSettings.DebugMode)
							TraceInfo("macro.loadControlProperties",
							          string.Format("Property added '{0}' with value '{1}'",
							                        mp.Key,
							                        propValue));
                    }
                    catch (Exception PropException)
                    {
						LogHelper.WarnWithException<macro>(string.Format(
										  "Error adding property '{0}' with value '{1}'",
										  mp.Key, propValue), PropException);

						if (GlobalSettings.DebugMode)
						{
							TraceWarn("macro.loadControlProperties",
							          string.Format(
								          "Error adding property '{0}' with value '{1}'",
								          mp.Key, propValue), PropException);
						}
                    }
                }

                // NH 06-01-2012: Only set value if it has content
                if (propValueSet)
                {
                    //GE 06-05-2012: Handle Nullable<T> properties
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.Name == "Nullable`1")
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                        if (underlyingType != null)
                        {
                            object safeValue = null;
                            if (propValue != null)
                            {
                                if (!(underlyingType == typeof(Guid)))
                                {
                                    prop.SetValue(control, Convert.ChangeType(propValue, underlyingType), null);
                                }
                                else
                                {
                                    Guid g = Guid.Empty;
                                    if (Guid.TryParse(string.Format("{0}", propValue), out g))
                                    {
                                        prop.SetValue(control, g, null);
                                    }
                                }
                            }
                            else
                            {
                                prop.SetValue(control, safeValue, null);
                            }
                        }
                    }
                    else
                    {
                        //GE 06-05-2012: Handle Guid properties as Convert.ChangeType throws on string->Guid
                        if (!(prop.PropertyType == typeof(Guid)))
                        {
                            prop.SetValue(control, Convert.ChangeType(propValue, prop.PropertyType), null);
                        }
                        else
                        {
                            Guid g = Guid.Empty;
                            if (Guid.TryParse(string.Format("{0}", propValue), out g))
                            {
                                prop.SetValue(control, g, null);
                            }
                        }
                    }
                }
            }
        }

	    /// <summary>
	    /// Loads an usercontrol using reflection into the macro object
	    /// </summary>
	    /// <param name="fileName">Filename of the usercontrol - ie. ~wulff.ascx</param>
	    /// <param name="model"> </param>
	    /// <param name="pageElements">The page elements.</param>
	    /// <returns></returns>
	    public Control loadUserControl(string fileName, MacroModel model, Hashtable pageElements)
        {
			Mandate.ParameterNotNullOrEmpty(fileName, "fileName");
	        Mandate.ParameterNotNull(model, "model");
			Mandate.ParameterNotNull(pageElements, "pageElements");

            try
            {
                string userControlPath = IOHelper.FindFile(fileName);

                if (!File.Exists(IOHelper.MapPath(userControlPath)))
                    throw new UmbracoException(string.Format("UserControl {0} does not exist.", fileName));

                var oControl = (UserControl)new UserControl().LoadControl(userControlPath);

                int slashIndex = fileName.LastIndexOf("/") + 1;
                if (slashIndex < 0)
                    slashIndex = 0;

                if (!String.IsNullOrEmpty(model.MacroControlIdentifier))
                    oControl.ID = model.MacroControlIdentifier;
                else
                    oControl.ID =
                        string.Format("{0}_{1}", fileName.Substring(slashIndex, fileName.IndexOf(".ascx") - slashIndex),
                                      StateHelper.GetContextValue<int>(macrosAddedKey));

                TraceInfo(loadUserControlKey, string.Format("Usercontrol added with id '{0}'", oControl.ID));

                Type type = oControl.GetType();

	            AddCurrentNodeToControl(oControl, type);
                updateControlProperties(type, oControl, model);
                return oControl;
            }
            catch (Exception e)
            {
				LogHelper.WarnWithException<macro>(string.Format("Error creating usercontrol ({0})", fileName), true, e);
                throw;
            }
        }

        private static void AddCurrentNodeToControl(Control control, Type type)
        {
            PropertyInfo currentNodeProperty = type.GetProperty("CurrentNode");
            if (currentNodeProperty != null && currentNodeProperty.CanWrite &&
                currentNodeProperty.PropertyType.IsAssignableFrom(typeof(Node)))
            {
                currentNodeProperty.SetValue(control, Node.GetCurrent(), null);
            }
            currentNodeProperty = type.GetProperty("currentNode");
            if (currentNodeProperty != null && currentNodeProperty.CanWrite &&
                currentNodeProperty.PropertyType.IsAssignableFrom(typeof(Node)))
            {
                currentNodeProperty.SetValue(control, Node.GetCurrent(), null);
            }
        }

        private static void TraceInfo(string category, string message)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write(category, message);
        }

        private static void TraceWarn(string category, string message)
        {
            if (HttpContext.Current != null)
				HttpContext.Current.Trace.Warn(category, message);
        }

		private static void TraceWarn(string category, string message, Exception ex)
		{
			if (HttpContext.Current != null)
				HttpContext.Current.Trace.Warn(category, message, ex);
		}

        public static string renderMacroStartTag(Hashtable attributes, int pageId, Guid versionId)
        {
            string div = "<div ";

            IDictionaryEnumerator ide = attributes.GetEnumerator();
            while (ide.MoveNext())
            {
                div += string.Format("umb_{0}=\"{1}\" ", ide.Key, encodeMacroAttribute((ide.Value ?? string.Empty).ToString()));
            }

            div += "ismacro=\"true\" onresizestart=\"return false;\" umbVersionId=\"" + versionId +
                   "\" umbPageid=\"" +
                   pageId +
                   "\" title=\"This is rendered content from macro\" class=\"umbMacroHolder\"><!-- startUmbMacro -->";

            return div;
        }

        private static string encodeMacroAttribute(string attributeContents)
        {
            // Replace linebreaks
            attributeContents = attributeContents.Replace("\n", "\\n").Replace("\r", "\\r");

            // Replace quotes
            attributeContents =
                attributeContents.Replace("\"", "&quot;");

            // Replace tag start/ends
            attributeContents =
                attributeContents.Replace("<", "&lt;").Replace(">", "&gt;");


            return attributeContents;
        }

        public static string renderMacroEndTag()
        {
            return "<!-- endUmbMacro --></div>";
        }

        public static string GetRenderedMacro(int MacroId, page umbPage, Hashtable attributes, int pageId)
        {
            macro m = GetMacro(MacroId);
            Control c = m.renderMacro(attributes, umbPage.Elements, pageId);
            TextWriter writer = new StringWriter();
            var ht = new HtmlTextWriter(writer);
            c.RenderControl(ht);
            string result = writer.ToString();

            // remove hrefs
            string pattern = "href=\"([^\"]*)\"";
            MatchCollection hrefs =
                Regex.Matches(result, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match href in hrefs)
                result = result.Replace(href.Value, "href=\"javascript:void(0)\"");

            return result;
        }

        public static string MacroContentByHttp(int PageID, Guid PageVersion, Hashtable attributes)
        {
            string tempAlias = (attributes["macroalias"] != null)
                                   ? attributes["macroalias"].ToString()
                                   : attributes["macroAlias"].ToString();
            macro currentMacro = GetMacro(tempAlias);
            if (!currentMacro.DontRenderInEditor)
            {
                string querystring = "umbPageId=" + PageID + "&umbVersionId=" + PageVersion;
                IDictionaryEnumerator ide = attributes.GetEnumerator();
                while (ide.MoveNext())
                    querystring += "&umb_" + ide.Key + "=" + HttpContext.Current.Server.UrlEncode((ide.Value ?? string.Empty).ToString());

                // Create a new 'HttpWebRequest' Object to the mentioned URL.
                string retVal = string.Empty;
                string protocol = GlobalSettings.UseSSL ? "https" : "http";
                string url = string.Format("{0}://{1}:{2}{3}/macroResultWrapper.aspx?{4}", protocol,
                                           HttpContext.Current.Request.ServerVariables["SERVER_NAME"],
                                           HttpContext.Current.Request.ServerVariables["SERVER_PORT"],
                                           IOHelper.ResolveUrl(SystemDirectories.Umbraco), querystring);

                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                // allows for validation of SSL conversations (to bypass SSL errors in debug mode!)
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

                // propagate the user's context
                // zb-00004 #29956 : refactor cookies names & handling
                HttpCookie inCookie = StateHelper.Cookies.UserContext.RequestCookie;
                var cookie = new Cookie(inCookie.Name, inCookie.Value, inCookie.Path,
                                        HttpContext.Current.Request.ServerVariables["SERVER_NAME"]);
                myHttpWebRequest.CookieContainer = new CookieContainer();
                myHttpWebRequest.CookieContainer.Add(cookie);

                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = null;
                try
                {
                    myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Stream streamResponse = myHttpWebResponse.GetResponseStream();
                        var streamRead = new StreamReader(streamResponse);
                        var readBuff = new Char[256];
                        int count = streamRead.Read(readBuff, 0, 256);
                        while (count > 0)
                        {
                            var outputData = new String(readBuff, 0, count);
                            retVal += outputData;
                            count = streamRead.Read(readBuff, 0, 256);
                        }
                        // Close the Stream object.
                        streamResponse.Close();
                        streamRead.Close();

                        // Find the content of a form
                        string grabStart = "<!-- grab start -->";
                        string grabEnd = "<!-- grab end -->";
                        int grabStartPos = retVal.IndexOf(grabStart) + grabStart.Length;
                        int grabEndPos = retVal.IndexOf(grabEnd) - grabStartPos;
                        retVal = retVal.Substring(grabStartPos, grabEndPos);
                    }
                    else
                        retVal = showNoMacroContent(currentMacro);

                    // Release the HttpWebResponse Resource.
                    myHttpWebResponse.Close();
                }
                catch (Exception)
                {
                    retVal = showNoMacroContent(currentMacro);
                }
                finally
                {
                    // Release the HttpWebResponse Resource.
                    if (myHttpWebResponse != null)
                        myHttpWebResponse.Close();
                }

                return retVal.Replace("\n", string.Empty).Replace("\r", string.Empty);
            }

            return showNoMacroContent(currentMacro);
        }

        private static string showNoMacroContent(macro currentMacro)
        {
            return "<span style=\"color: green\"><strong>" + currentMacro.Name +
                   "</strong><br />No macro content available for WYSIWYG editing</span>";
        }

        private static bool ValidateRemoteCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors policyErrors
            )
        {
            if (GlobalSettings.DebugMode)
            {
                // allow any old dodgy certificate...
                return true;
            }
            else
            {
                return policyErrors == SslPolicyErrors.None;
            }
        }

        /// <summary>
        /// Adds the XSLT extension namespaces to the XSLT header using 
        /// {0} as the container for the namespace references and
        /// {1} as the container for the exclude-result-prefixes
        /// </summary>
        /// <param name="xslt">The XSLT</param>
        /// <returns></returns>
        public static string AddXsltExtensionsToHeader(string xslt)
        {
            var namespaceList = new StringBuilder();
            var namespaceDeclaractions = new StringBuilder();
            foreach (var extension in GetXsltExtensions())
            {
                namespaceList.Append(extension.Key).Append(' ');
                namespaceDeclaractions.AppendFormat("xmlns:{0}=\"urn:{0}\" ", extension.Key);
            }

            // parse xslt
            xslt = xslt.Replace("{0}", namespaceDeclaractions.ToString());
            xslt = xslt.Replace("{1}", namespaceList.ToString());
            return xslt;
        }

        [Obsolete("Please stop using these as they'll be removed in v4.8")]
        public static bool TryGetColumnString(IRecordsReader reader, string columnName, out string value)
        {
            if (reader.ContainsField(columnName) && !reader.IsNull(columnName))
            {
                value = reader.GetString(columnName);
                return true;
            }

            value = string.Empty;
            return false;
        }

        [Obsolete("Please stop using these as they'll be removed in v4.8")]
        public static bool TryGetColumnInt32(IRecordsReader reader, string columnName, out int value)
        {
            if (reader.ContainsField(columnName) && !reader.IsNull(columnName))
            {
                value = reader.GetInt(columnName);
                return true;
            }

            value = -1;
            return false;
        }

        [Obsolete("Please stop using these as they'll be removed in v4.8")]
        public static bool TryGetColumnBool(IRecordsReader reader, string columnName, out bool value)
        {
            if (reader.ContainsField(columnName) && !reader.IsNull(columnName))
            {
                value = reader.GetBoolean(columnName);
                return true;
            }

            value = false;
            return false;
        }

        #region Events

        /// <summary>
        /// Occurs when a macro error is raised.
        /// </summary>
        public static event EventHandler<MacroErrorEventArgs> Error;

        /// <summary>
        /// Raises the <see cref="MacroErrorEventArgs"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MacroErrorEventArgs"/> instance containing the event data.</param>
        protected void OnError(MacroErrorEventArgs e)
        {
            if (Error != null)
            {
                Error(this, e);
            }
        }

        #endregion
    }
}