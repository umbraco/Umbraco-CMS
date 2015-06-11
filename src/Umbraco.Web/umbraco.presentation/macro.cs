using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using System.Xml.XPath;
using System.Xml.Xsl;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.Xml.XPath;
using Umbraco.Core.Profiling;
using umbraco.interfaces;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Macros;
using Umbraco.Web.Models;
using Umbraco.Web.Templates;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.macro;
using umbraco.DataLayer;
using umbraco.NodeFactory;
using umbraco.presentation.templateControls;
using Umbraco.Web.umbraco.presentation;
using Content = umbraco.cms.businesslogic.Content;
using Macro = umbraco.cms.businesslogic.macro.Macro;
using MacroErrorEventArgs = Umbraco.Core.Events.MacroErrorEventArgs;
using System.Linq;
using File = System.IO.File;
using MacroTypes = umbraco.cms.businesslogic.macro.MacroTypes;
using Member = umbraco.cms.businesslogic.member.Member;

namespace umbraco
{
    /// <summary>
    /// Summary description for macro.
    /// </summary>
    public class macro
    {
        #region private properties

        /// <summary>Cache for <see cref="GetPredefinedXsltExtensions"/>.</summary>
        private static Dictionary<string, object> _predefinedExtensions;
        private static XsltSettings _xsltSettings;
        private const string LoadUserControlKey = "loadUserControl";
        private readonly StringBuilder _content = new StringBuilder();
        private const string MacrosAddedKey = "macrosAdded";
        public IList<Exception> Exceptions = new List<Exception>();
        
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        static macro()
        {
            _xsltSettings = GlobalSettings.ApplicationTrustLevel > AspNetHostingPermissionLevel.Medium
                ? XsltSettings.TrustedXslt
                : XsltSettings.Default;
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
            set { _content.Append(value); }
            get { return _content.ToString(); }
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
            return new macro(alias);
        }

        public static macro GetMacro(int id)
        {        
            return new macro(id);
        }

        #endregion

        private const string XsltExtensionsCacheKey = "UmbracoXsltExtensions";

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

        /// <summary>
        /// Deletes macro definition from cache.
        /// </summary>
        /// <returns>True if succesfull, false if nothing has been removed</returns>
        [Obsolete("Use DistributedCache.Instance.RemoveMacroCache instead, macro cache will automatically be cleared and shouldn't need to be manually cleared.")]
        public bool removeFromCache()
        {
            if (this.Model != null)
            {
                DistributedCache.Instance.RemoveMacroCache(this);    
            }
            
            //this always returned false... hrm. oh well i guess we leave it like that
            return false;
        }

        string GetCacheIdentifier(MacroModel model, Hashtable pageElements, int pageId)
        {
            var id = new StringBuilder();

            var alias = string.IsNullOrEmpty(model.ScriptCode) ? model.Alias : Macro.GenerateCacheKeyFromCode(model.ScriptCode);
            id.AppendFormat("{0}-", alias);

            if (CacheByPage)
            {
                id.AppendFormat("{0}-", pageId);
            }

            if (CacheByPersonalization)
            {
                object memberId = 0;
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var provider = Umbraco.Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    var member = Umbraco.Core.Security.MembershipProviderExtensions.GetCurrentUser(provider);
                    if (member != null)
                    {
                        memberId = member.ProviderUserKey ?? 0;
                    }
                }
                id.AppendFormat("m{0}-", memberId);
            }

            foreach (var prop in model.Properties)
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

        /// <summary>
        /// An event that is raised just before the macro is rendered allowing developers to modify the macro before it executes.
        /// </summary>
        public static event TypedEventHandler<macro, MacroRenderingEventArgs> MacroRendering;
        
        /// <summary>
        /// Raises the MacroRendering event
        /// </summary>
        /// <param name="e"></param>
        protected void OnMacroRendering(MacroRenderingEventArgs e)
        {
            if (MacroRendering != null)
                MacroRendering(this, e);
        }

        /// <summary>
        /// Renders the macro
        /// </summary>
        /// <param name="pageElements"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public Control renderMacro(Hashtable pageElements, int pageId)
        {
            // Event to allow manipulation of Macro Model
            OnMacroRendering(new MacroRenderingEventArgs(pageElements, pageId));

            var macroInfo = (Model.MacroType == MacroTypes.Script && Model.Name.IsNullOrWhiteSpace())
                                ? string.Format("Render Inline Macro, Cache: {0})", Model.CacheDuration)
                                : string.Format("Render Macro: {0}, type: {1}, cache: {2})", Name, Model.MacroType, Model.CacheDuration);

            using (DisposableTimer.DebugDuration<macro>(macroInfo))
            {
                TraceInfo("renderMacro", macroInfo, excludeProfiling:true);

                StateHelper.SetContextValue(MacrosAddedKey, StateHelper.GetContextValue<int>(MacrosAddedKey) + 1);

                // zb-00037 #29875 : parse attributes here (and before anything else)
                foreach (MacroPropertyModel prop in Model.Properties)
                    prop.Value = helper.parseAttribute(pageElements, prop.Value);

                Model.CacheIdentifier = GetCacheIdentifier(Model, pageElements, pageId);

                string macroHtml;
                Control macroControl;
                //get the macro from cache if it is there
                GetMacroFromCache(out macroHtml, out macroControl);

                // FlorisRobbemont: Empty macroHtml (not null, but "") doesn't mean a re-render is necessary
                if (macroHtml == null && macroControl == null)
                {
                    var renderFailed = false;
                    var macroType = Model.MacroType != MacroTypes.Unknown
                                        ? (int) Model.MacroType
                                        : MacroType;

                    switch (macroType)
                    {
                        case (int) MacroTypes.PartialView:

                            //error handler for partial views, is an action because we need to re-use it twice below
                            Func<Exception, Control> handleError = e =>
                                {
                                    LogHelper.WarnWithException<macro>("Error loading Partial View (file: " + ScriptFile + ")", true, e);
                                    // Invoke any error handlers for this macro
                                    var macroErrorEventArgs =
                                        new MacroErrorEventArgs
                                            {
                                                Name = Model.Name,
                                                Alias = Model.Alias,
                                                ItemKey = Model.ScriptName,
                                                Exception = e,
                                                Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour
                                            };
                                    return GetControlForErrorBehavior("Error loading Partial View script (file: " + ScriptFile + ")", macroErrorEventArgs);
                                };

                            using (DisposableTimer.DebugDuration<macro>("Executing Partial View: " + Model.TypeName))
                            {
                                TraceInfo("umbracoMacro", "Partial View added (" + Model.TypeName + ")", excludeProfiling:true);
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
                                    LogHelper.WarnWithException<macro>(
                                        "Error loading partial view macro (View: " + Model.ScriptName + ")", true, e);

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
                            }
                        case (int) MacroTypes.UserControl:

                            using (DisposableTimer.DebugDuration<macro>("Executing UserControl: " + Model.TypeName))
                            {
                                try
                                {
                                    TraceInfo("umbracoMacro", "Usercontrol added (" + Model.TypeName + ")", excludeProfiling:true);

                                    // Add tilde for v4 defined macros
                                    if (string.IsNullOrEmpty(Model.TypeName) == false &&
                                        Model.TypeName.StartsWith("~") == false)
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
                                    var macroErrorEventArgs = new MacroErrorEventArgs
                                    {
                                        Name = Model.Name,
                                        Alias = Model.Alias,
                                        ItemKey = Model.TypeName,
                                        Exception = e,
                                        Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour
                                    };

                                    macroControl = GetControlForErrorBehavior("Error loading userControl '" + Model.TypeName + "'", macroErrorEventArgs);
                                    //if it is null, then we are supposed to throw the (original) exception
                                    // see: http://issues.umbraco.org/issue/U4-497 at the end
                                    if (macroControl == null)
                                    {
                                        throw;
                                    }

                                    break;
                                }
                            }
                            
                        case (int) MacroTypes.CustomControl:

                            using (DisposableTimer.DebugDuration<macro>("Executing CustomControl: " + Model.TypeName + "." + Model.TypeAssembly))
                            {
                                try
                                {
                                    TraceInfo("umbracoMacro", "Custom control added (" + Model.TypeName + "), ScriptAssembly: " + Model.TypeAssembly, excludeProfiling: true);
                                    macroControl = loadControl(Model.TypeAssembly, ScriptType, Model, pageElements);
                                    break;
                                }
                                catch (Exception e)
                                {
                                    renderFailed = true;
                                    Exceptions.Add(e);

                                    LogHelper.WarnWithException<macro>(
                                        "Error loading customControl (Assembly: " + Model.TypeAssembly + ", Type: '" +
                                        Model.TypeName + "'", true, e);

                                    // Invoke any error handlers for this macro
                                    var macroErrorEventArgs = new MacroErrorEventArgs
                                    {
                                        Name = Model.Name,
                                        Alias = Model.Alias,
                                        ItemKey = Model.TypeAssembly,
                                        Exception = e,
                                        Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour
                                    };

                                    macroControl = GetControlForErrorBehavior("Error loading customControl (Assembly: " + Model.TypeAssembly + ", Type: '" + Model.TypeName + "'", macroErrorEventArgs);
                                    //if it is null, then we are supposed to throw the (original) exception
                                    // see: http://issues.umbraco.org/issue/U4-497 at the end
                                    if (macroControl == null)
                                    {
                                        throw;
                                    }

                                    break;
                                }
                            }
                        case (int) MacroTypes.XSLT:                            
                            macroControl = LoadMacroXslt(this, Model, pageElements, true);
                            break;                                                           
                        case (int) MacroTypes.Script:

                            //error handler for partial views, is an action because we need to re-use it twice below
                            Func<Exception, Control> handleMacroScriptError = e =>
                                {
                                    LogHelper.WarnWithException<macro>("Error loading MacroEngine script (file: " + ScriptFile + ", Type: '" + Model.TypeName + "'", true, e);

                                    // Invoke any error handlers for this macro
                                    var macroErrorEventArgs =
                                        new MacroErrorEventArgs
                                            {
                                                Name = Model.Name,
                                                Alias = Model.Alias,
                                                ItemKey = ScriptFile,
                                                Exception = e,
                                                Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour
                                            };

                                    return GetControlForErrorBehavior("Error loading MacroEngine script (file: " + ScriptFile + ")", macroErrorEventArgs);
                                };

                            using (DisposableTimer.DebugDuration<macro>("Executing MacroEngineScript: " + ScriptFile))
                            {
                                try
                                {
                                    TraceInfo("umbracoMacro", "MacroEngine script added (" + ScriptFile + ")", excludeProfiling: true);
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
                            }
                        case (int) MacroTypes.Unknown:
                        default:
                            if (GlobalSettings.DebugMode)
                            {
                                macroControl = new LiteralControl("&lt;Macro: " + Name + " (" + ScriptAssembly + "," + ScriptType + ")&gt;");
                            }
                            break;
                    }

                    //add to cache if render is successful
                    if (renderFailed == false)
                    {
                        macroControl = AddMacroResultToCache(macroControl);
                    }

                }
                else if (macroControl == null)
                {
                    macroControl = new LiteralControl(macroHtml);
                }

                return macroControl;
            }
        }

        /// <summary>
        /// Adds the macro result to cache and returns the control since it might be updated
        /// </summary>
        /// <param name="macroControl"></param>
        /// <returns></returns>
        private Control AddMacroResultToCache(Control macroControl)
        {
            // Add result to cache if successful
            if (Model.CacheDuration > 0)
            {
                // do not add to cache if there's no member and it should cache by personalization
                if (!Model.CacheByMember || (Model.CacheByMember && Member.IsLoggedOn()))
                {
                    if (macroControl != null)
                    {
                        string dateAddedCacheKey;

                        using (DisposableTimer.DebugDuration<macro>("Saving MacroContent To Cache: " + Model.CacheIdentifier))
                        {

                            // NH: Scripts and XSLT can be generated as strings, but not controls as page events wouldn't be hit (such as Page_Load, etc)
                            if (CacheMacroAsString(Model))
                            {
                                var outputCacheString = "";

                                using (var sw = new StringWriter())
                                {
                                    var hw = new HtmlTextWriter(sw);
                                    macroControl.RenderControl(hw);

                                    outputCacheString = sw.ToString();
                                }

                                //insert the cache string result
                                ApplicationContext.Current.ApplicationCache.InsertCacheItem(
                                    CacheKeys.MacroHtmlCacheKey + Model.CacheIdentifier,
                                    CacheItemPriority.NotRemovable,
                                    new TimeSpan(0, 0, Model.CacheDuration),
                                    () => outputCacheString);

                                dateAddedCacheKey = CacheKeys.MacroHtmlDateAddedCacheKey + Model.CacheIdentifier;

                                // zb-00003 #29470 : replace by text if not already text
                                // otherwise it is rendered twice
                                if (!(macroControl is LiteralControl))
                                    macroControl = new LiteralControl(outputCacheString);

                                TraceInfo("renderMacro",
                                          string.Format("Macro Content saved to cache '{0}'.", Model.CacheIdentifier));
                            }
                            else
                            {
                                //insert the cache control result
                                ApplicationContext.Current.ApplicationCache.InsertCacheItem(
                                    CacheKeys.MacroControlCacheKey + Model.CacheIdentifier,
                                    CacheItemPriority.NotRemovable,
                                    new TimeSpan(0, 0, Model.CacheDuration),
                                    () => new MacroCacheContent(macroControl, macroControl.ID));

                                dateAddedCacheKey = CacheKeys.MacroControlDateAddedCacheKey + Model.CacheIdentifier;

                                TraceInfo("renderMacro",
                                          string.Format("Macro Control saved to cache '{0}'.", Model.CacheIdentifier));
                            }

                            //insert the date inserted (so we can check file modification date)
                            ApplicationContext.Current.ApplicationCache.InsertCacheItem(
                                dateAddedCacheKey,
                                CacheItemPriority.NotRemovable,
                                new TimeSpan(0, 0, Model.CacheDuration),
                                () => DateTime.Now);
                            
                        }
                        
                    }
                }
            }

            return macroControl;
        }

        /// <summary>
        /// Returns the cached version of this macro either as a string or as a Control
        /// </summary>
        /// <param name="macroHtml"></param>
        /// <param name="macroControl"></param>
        /// <returns></returns>
        /// <remarks>
        /// Depending on the type of macro, this will return the result as a string or as a control. This also 
        /// checks to see if preview mode is activated, if it is then we don't return anything from cache.
        /// </remarks>
        private void GetMacroFromCache(out string macroHtml, out Control macroControl)
        {
            macroHtml = null;
            macroControl = null;

            if (UmbracoContext.Current.InPreviewMode == false && Model.CacheDuration > 0)
            {
                var macroFile = GetMacroFile(Model);
                var fileInfo = new FileInfo(HttpContext.Current.Server.MapPath(macroFile));

                if (CacheMacroAsString(Model))
                {
                    macroHtml = ApplicationContext.Current.ApplicationCache.GetCacheItem<string>(
                        CacheKeys.MacroHtmlCacheKey + Model.CacheIdentifier);

                    // FlorisRobbemont: 
                    // An empty string means: macroHtml has been cached before, but didn't had any output (Macro doesn't need to be rendered again)
                    // An empty reference (null) means: macroHtml has NOT been cached before
                    if (macroHtml != null)
                    {
                        if (MacroNeedsToBeClearedFromCache(Model, CacheKeys.MacroHtmlDateAddedCacheKey + Model.CacheIdentifier, fileInfo))
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
                    var cacheContent = ApplicationContext.Current.ApplicationCache.GetCacheItem<MacroCacheContent>(
                        CacheKeys.MacroControlCacheKey + Model.CacheIdentifier);

                    if (cacheContent != null)
                    {
                        macroControl = cacheContent.Content;
                        macroControl.ID = cacheContent.ID;

                        if (MacroNeedsToBeClearedFromCache(Model, CacheKeys.MacroControlDateAddedCacheKey + Model.CacheIdentifier, fileInfo))
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
        /// <param name="model"></param>
        /// <param name="dateAddedKey"></param>
        /// <param name="macroFile"></param>
        /// <returns></returns>
        /// <remarks>
        /// The only reason this is necessary is because a developer might update a file associated with the 
        /// macro, we need to ensure if that is the case that the cache not be used and it is refreshed.
        /// </remarks>
        internal static bool MacroNeedsToBeClearedFromCache(MacroModel model, string dateAddedKey, FileInfo macroFile)
        {
            if (MacroIsFileBased(model))
            {
                var cacheResult = ApplicationContext.Current.ApplicationCache.GetCacheItem<DateTime?>(dateAddedKey);

                if (cacheResult != null)
                {
                    var dateMacroAdded = cacheResult;

                    if (macroFile.LastWriteTime.CompareTo(dateMacroAdded) == 1)
                    {
                        TraceInfo("renderMacro", string.Format("Macro needs to be removed from cache due to file change '{0}'.", model.CacheIdentifier));
                        return true;
                    }
                }
            }

            return false;
        }

        internal static string GetMacroFile(MacroModel model)
        {
            switch (model.MacroType)
            {
                case MacroTypes.XSLT:
                    return string.Concat("~/xslt/", model.Xslt);                
                case MacroTypes.Python:
                case MacroTypes.Script:
                    return string.Concat("~/macroScripts/", model.ScriptName);
                case MacroTypes.PartialView:
                    return model.ScriptName; //partial views are saved with the full virtual path
                case MacroTypes.UserControl:
                    return model.TypeName; //user controls saved with the full virtual path
                case MacroTypes.CustomControl:                
                case MacroTypes.Unknown:
                default:
                    return "/" + model.TypeName;
            }
        }

        internal static bool MacroIsFileBased(MacroModel model)
        {
            return model.MacroType != MacroTypes.CustomControl && model.MacroType != MacroTypes.Unknown;
        }

        /// <summary>
        /// Determine if macro can be cached as string
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Scripts and XSLT can be generated as strings, but not controls as page events wouldn't be hit (such as Page_Load, etc)
        /// </remarks>
        internal static bool CacheMacroAsString(MacroModel model)
        {
            switch (model.MacroType)
            {
                case MacroTypes.XSLT:            
                case MacroTypes.Python:
                case MacroTypes.Script:
                case MacroTypes.PartialView:
                    return true;
                case MacroTypes.UserControl:
                case MacroTypes.CustomControl:
                case MacroTypes.Unknown:
                default:
                    return false;
            }
        }

        public static XslCompiledTransform getXslt(string XsltFile)
        {
            //TODO: SD: Do we really need to cache this??
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                CacheKeys.MacroXsltCacheKey + XsltFile,
                CacheItemPriority.Default,
                new CacheDependency(IOHelper.MapPath(SystemDirectories.Xslt + "/" + XsltFile)),
                () =>
                    {
                        using (var xslReader = new XmlTextReader(IOHelper.MapPath(SystemDirectories.Xslt.EnsureEndsWith('/') + XsltFile)))
                        {
                            return CreateXsltTransform(xslReader, GlobalSettings.DebugMode);
                        }
                    });
        }

        public void UpdateMacroModel(Hashtable attributes)
        {
            foreach (MacroPropertyModel mp in Model.Properties)
            {
                if (attributes.ContainsKey(mp.Key.ToLowerInvariant()))
                {
                    var item = attributes[mp.Key.ToLowerInvariant()];

                    mp.Value = item == null ? string.Empty : item.ToString();
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
            var macroXslt = new XslCompiledTransform(debugMode);
            var xslResolver = new XmlUrlResolver
                {
                    Credentials = CredentialCache.DefaultCredentials
                };

            xslReader.EntityHandling = EntityHandling.ExpandEntities;

            try
            {
                macroXslt.Load(xslReader, _xsltSettings, xslResolver);
            }
            finally
            {
                xslReader.Close();
            }

            return macroXslt;
        }

        [Obsolete("This is no longer used in the codebase and will be removed in future versions")]
        public static void unloadXslt(string XsltFile)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.MacroXsltCacheKey + XsltFile);
        }

        #region LoadMacroXslt

        // gets the control for the macro, using GetXsltTransform methods for execution
        // will pick XmlDocument or Navigator mode depending on the capabilities of the published caches
        internal Control LoadMacroXslt(macro macro, MacroModel model, Hashtable pageElements, bool throwError)
        {
            if (XsltFile.Trim() == string.Empty)
            {
                TraceWarn("macro", "Xslt is empty");
                return new LiteralControl(string.Empty);
            }

            using (DisposableTimer.DebugDuration<macro>("Executing XSLT: " + XsltFile))
            {
                XmlDocument macroXml = null;
                MacroNavigator macroNavigator = null;
                NavigableNavigator contentNavigator = null;

                var canNavigate =
                    UmbracoContext.Current.ContentCache.XPathNavigatorIsNavigable &&
                    UmbracoContext.Current.MediaCache.XPathNavigatorIsNavigable;

                if (!canNavigate)
                {
                     // get master xml document
                    var cache = UmbracoContext.Current.ContentCache.InnerCache as Umbraco.Web.PublishedCache.XmlPublishedCache.PublishedContentCache;
                    if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
                    XmlDocument umbracoXml = cache.GetXml(UmbracoContext.Current, UmbracoContext.Current.InPreviewMode);
                    macroXml = new XmlDocument();
                    macroXml.LoadXml("<macro/>");
                    foreach (var prop in macro.Model.Properties)
                    {
                        AddMacroXmlNode(umbracoXml, macroXml, prop.Key, prop.Type, prop.Value);
                    }
                }
                else
                {
                    var parameters = new List<MacroNavigator.MacroParameter>();
                    contentNavigator = UmbracoContext.Current.ContentCache.GetXPathNavigator() as NavigableNavigator;
                    var mediaNavigator = UmbracoContext.Current.MediaCache.GetXPathNavigator() as NavigableNavigator;
                    foreach (var prop in macro.Model.Properties)
                    {
                        AddMacroParameter(parameters, contentNavigator, mediaNavigator, prop.Key, prop.Type, prop.Value);
                    }
                    macroNavigator = new MacroNavigator(parameters);
                }

                if (HttpContext.Current.Request.QueryString["umbDebug"] != null && GlobalSettings.DebugMode)
                {
                    var outerXml = macroXml == null ? macroNavigator.OuterXml : macroXml.OuterXml;
                    return
                        new LiteralControl("<div style=\"border: 2px solid green; padding: 5px;\"><b>Debug from " +
                                           macro.Name +
                                           "</b><br/><p>" + HttpContext.Current.Server.HtmlEncode(outerXml) +
                                           "</p></div>");
                }

                try
                {
                    var xsltFile = getXslt(XsltFile);

                    using (DisposableTimer.DebugDuration<macro>("Performing transformation"))
                    {
                        try
                        {
                        var transformed = canNavigate
                            ? GetXsltTransformResult(macroNavigator, contentNavigator, xsltFile) // better?
                            : GetXsltTransformResult(macroXml, xsltFile); // document
                            var result = CreateControlsFromText(transformed);

                            return result;
                        }
                        catch (Exception e)
                        {
                            Exceptions.Add(e);
                            LogHelper.WarnWithException<macro>("Error parsing XSLT file", e);
                            
                            var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = Model.Xslt, Exception = e, Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour };
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
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    LogHelper.WarnWithException<macro>("Error loading XSLT " + Model.Xslt, true, e);

                    // Invoke any error handlers for this macro
                    var macroErrorEventArgs = new MacroErrorEventArgs { Name = Model.Name, Alias = Model.Alias, ItemKey = Model.Xslt, Exception = e, Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour };
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
        }

        // gets the control for the macro, using GetXsltTransform methods for execution
        public Control loadMacroXSLT(macro macro, MacroModel model, Hashtable pageElements)
        {
            return LoadMacroXslt(macro, model, pageElements, false);
        }

        #endregion

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

        #region GetXsltTransform

        // gets the result of the xslt transform with no parameters - XmlDocument mode
        public static string GetXsltTransformResult(XmlDocument macroXml, XslCompiledTransform xslt)
        {
            return GetXsltTransformResult(macroXml, xslt, null);
        }

        // gets the result of the xslt transform - XmlDocument mode
        public static string GetXsltTransformResult(XmlDocument macroXml, XslCompiledTransform xslt, Dictionary<string, object> parameters)
        {
            TextWriter tw = new StringWriter();

            XsltArgumentList xslArgs;

            using (DisposableTimer.DebugDuration<macro>("Adding XSLT Extensions"))
            {                
                xslArgs = AddXsltExtensions();
                var lib = new library();
                xslArgs.AddExtensionObject("urn:umbraco.library", lib);
            }
            
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
            using (DisposableTimer.DebugDuration<macro>("Executing XSLT transform"))
            {
                xslt.Transform(macroXml.CreateNavigator(), xslArgs, tw);
            }                       
			return TemplateUtilities.ResolveUrlsFromTextString(tw.ToString());
        }

        // gets the result of the xslt transform with no parameters - Navigator mode
        public static string GetXsltTransformResult(XPathNavigator macroNavigator, XPathNavigator contentNavigator,
            XslCompiledTransform xslt)
        {
            return GetXsltTransformResult(macroNavigator, contentNavigator, xslt, null);
        }

        // gets the result of the xslt transform - Navigator mode
        public static string GetXsltTransformResult(XPathNavigator macroNavigator, XPathNavigator contentNavigator,
            XslCompiledTransform xslt, Dictionary<string, object> parameters)
        {
            TextWriter tw = new StringWriter();

            XsltArgumentList xslArgs;
            using (DisposableTimer.DebugDuration<macro>("Adding XSLT Extensions"))
            {
                xslArgs = AddXsltExtensions();
                var lib = new library();
                xslArgs.AddExtensionObject("urn:umbraco.library", lib);
            }

            // Add parameters
            if (parameters == null || !parameters.ContainsKey("currentPage"))
            {
                var current = contentNavigator.Clone().Select("//* [@id=" + HttpContext.Current.Items["pageID"] + "]");
                xslArgs.AddParam("currentPage", string.Empty, current);
            }
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                    xslArgs.AddParam(parameter.Key, string.Empty, parameter.Value);
            }

            // Do transformation
            using (DisposableTimer.DebugDuration<macro>("Executing XSLT transform"))
            {
                xslt.Transform(macroNavigator, xslArgs, tw);
            }
            return TemplateUtilities.ResolveUrlsFromTextString(tw.ToString());
        }

        #endregion

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
            return XsltExtensionsResolver.Current.XsltExtensions
                                         .ToDictionary(x => x.Namespace, x => x.ExtensionObject);
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

        #region LoadMacroXslt (2)

        // add elements to the <macro> root node, corresponding to parameters
        private void AddMacroXmlNode(XmlDocument umbracoXml, XmlDocument macroXml,
            string macroPropertyAlias, string macroPropertyType, string macroPropertyValue)
        {
            XmlNode macroXmlNode = macroXml.CreateNode(XmlNodeType.Element, macroPropertyAlias, string.Empty);
            var x = new XmlDocument();

            // if no value is passed, then use the current "pageID" as value
            var contentId = macroPropertyValue == string.Empty ? UmbracoContext.Current.PageId.ToString() : macroPropertyValue;

	        TraceInfo("umbracoMacro",
	                  "Xslt node adding search start (" + macroPropertyAlias + ",'" +
	                  macroPropertyValue + "')");
            
            //TODO: WE need to fix this so that we give control of this stuff over to the actual parameter editors!
            
            switch (macroPropertyType)
            {
                case "contentTree":
                    var nodeId = macroXml.CreateAttribute("nodeID");
                    nodeId.Value = contentId;
                    macroXmlNode.Attributes.SetNamedItem(nodeId);

                    // Get subs
                    try
                    {
                        macroXmlNode.AppendChild(macroXml.ImportNode(umbracoXml.GetElementById(contentId), true));
                    }
                    catch
                    { }
                    break;

                case "contentCurrent":
                    var importNode = macroPropertyValue == string.Empty
                        ? umbracoXml.GetElementById(contentId)
                        : umbracoXml.GetElementById(macroPropertyValue);

                    var currentNode = macroXml.ImportNode(importNode, true);

                    // remove all sub content nodes
                    foreach (XmlNode n in currentNode.SelectNodes("node|*[@isDoc]"))
                        currentNode.RemoveChild(n);

                    macroXmlNode.AppendChild(currentNode);

                    break;                    

                case "contentAll":
                    macroXmlNode.AppendChild(macroXml.ImportNode(umbracoXml.DocumentElement, true));
                    break;

                case "contentRandom":
                    XmlNode source = umbracoXml.GetElementById(contentId);
					if (source != null)
					{
						var sourceList = source.SelectNodes("node|*[@isDoc]");
						if (sourceList.Count > 0)
						{
							int rndNumber;
							var r = library.GetRandom();
							lock (r)
							{
								rndNumber = r.Next(sourceList.Count);
							}
							var node = macroXml.ImportNode(sourceList[rndNumber], true);
							// remove all sub content nodes
							foreach (XmlNode n in node.SelectNodes("node|*[@isDoc]"))
								node.RemoveChild(n);

							macroXmlNode.AppendChild(node);
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
                    if (string.IsNullOrEmpty(macroPropertyValue) == false)
                    {
                        var c = new Content(int.Parse(macroPropertyValue));
                        macroXmlNode.AppendChild(macroXml.ImportNode(c.ToXml(umbraco.content.Instance.XmlContent, false), true));
                    }
                    break;

                default:
                    macroXmlNode.InnerText = HttpContext.Current.Server.HtmlDecode(macroPropertyValue);
                    break;
            }
            macroXml.FirstChild.AppendChild(macroXmlNode);
        }

        // add parameters to the macro parameters collection
        private void AddMacroParameter(ICollection<MacroNavigator.MacroParameter> parameters,
            NavigableNavigator contentNavigator, NavigableNavigator mediaNavigator,
            string macroPropertyAlias,string macroPropertyType, string macroPropertyValue)
        {
            // if no value is passed, then use the current "pageID" as value
            var contentId = macroPropertyValue == string.Empty ? UmbracoContext.Current.PageId.ToString() : macroPropertyValue;

	        TraceInfo("umbracoMacro",
	                  "Xslt node adding search start (" + macroPropertyAlias + ",'" +
	                  macroPropertyValue + "')");

            // beware! do not use the raw content- or media- navigators, but clones !!

            switch (macroPropertyType)
            {
                case "contentTree":
                    parameters.Add(new MacroNavigator.MacroParameter(
                        macroPropertyAlias,
                        contentNavigator.CloneWithNewRoot(contentId), // null if not found - will be reported as empty
                        attributes: new Dictionary<string, string> { { "nodeID", contentId } }));

                    break;

                case "contentPicker":
                    parameters.Add(new MacroNavigator.MacroParameter(
                        macroPropertyAlias,
                        contentNavigator.CloneWithNewRoot(contentId), // null if not found - will be reported as empty
                        0));
                    break;

                case "contentSubs":
                    parameters.Add(new MacroNavigator.MacroParameter(
                        macroPropertyAlias,
                        contentNavigator.CloneWithNewRoot(contentId), // null if not found - will be reported as empty
                        1));
                    break;

                case "contentAll":
                    parameters.Add(new MacroNavigator.MacroParameter(macroPropertyAlias, contentNavigator.Clone()));
                    break;

                case "contentRandom":
                    var nav = contentNavigator.Clone();
                    if (nav.MoveToId(contentId))
                    {
                        var descendantIterator = nav.Select("./* [@isDoc]");
                        if (descendantIterator.MoveNext())
                        {
                            // not empty - and won't change
                            var descendantCount = descendantIterator.Count;

                            int index;
                            var r = library.GetRandom();
                            lock (r)
                            {
                                index = r.Next(descendantCount);
                            }

                            while (index > 0 && descendantIterator.MoveNext())
                                index--;

                            var node = descendantIterator.Current.UnderlyingObject as INavigableContent;
                            if (node != null)
                            {
                                nav = contentNavigator.CloneWithNewRoot(node.Id.ToString(CultureInfo.InvariantCulture));
                                parameters.Add(new MacroNavigator.MacroParameter(macroPropertyAlias, nav, 0));                                
                            }
                            else
                                throw new InvalidOperationException("Iterator contains non-INavigableContent elements.");
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
                    parameters.Add(new MacroNavigator.MacroParameter(
                        macroPropertyAlias,
                        mediaNavigator.CloneWithNewRoot(contentId), // null if not found - will be reported as empty
                        0));
                    break;

                default:
                    parameters.Add(new MacroNavigator.MacroParameter(macroPropertyAlias, HttpContext.Current.Server.HtmlDecode(macroPropertyValue)));
                    break;
            }
        }

        #endregion

		/// <summary>
		/// Renders a Partial View Macro
		/// </summary>
		/// <param name="macro"></param>
		/// <returns></returns>
		internal ScriptingMacroResult LoadPartialViewMacro(MacroModel macro)
		{
			var retVal = new ScriptingMacroResult();
			IMacroEngine engine = null;
			
			engine = MacroEngineFactory.GetEngine(PartialViewMacroEngine.EngineName);
			var ret = engine.Execute(macro, GetCurrentNode());

			// if the macro engine supports success reporting and executing failed, then return an empty control so it's not cached
			if (engine is IMacroEngineResultStatus)
			{
				var result = engine as IMacroEngineResultStatus;
				if (!result.Success)
				{
					retVal.ResultException = result.ResultException;
				}
			}
			retVal.Result = ret;
			return retVal;
		}

        public ScriptingMacroResult loadMacroScript(MacroModel macro)
        {
            var retVal = new ScriptingMacroResult();
            string ret = String.Empty;
            IMacroEngine engine = null;
            if (!String.IsNullOrEmpty(macro.ScriptCode))
            {
                engine = MacroEngineFactory.GetByExtension(macro.ScriptLanguage);
                ret = engine.Execute(
                    macro,
                    GetCurrentNode());
            }
            else
            {
                string path = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + macro.ScriptName);
                engine = MacroEngineFactory.GetByFilename(path);
                ret = engine.Execute(macro, GetCurrentNode());
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
            retVal.Result = ret;
            return retVal;
        }
        
        /// <summary>
        /// Loads a custom or webcontrol using reflection into the macro object
        /// </summary>
        /// <param name="fileName">The assembly to load from</param>
        /// <param name="controlName">Name of the control</param>
        /// <returns></returns>
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

            AddCurrentNodeToControl(control);

            // Properties
            UpdateControlProperties(control, model);
            return control;
        }

        //TODO: SD : We *really* need to get macro's rendering properly with a real macro engine
        // and move logic like this (after it is completely overhauled) to a UserControlMacroEngine.
        internal static void UpdateControlProperties(Control control, MacroModel model)
        {
            var type = control.GetType();

            foreach (var mp in model.Properties)
            {
                var prop = type.GetProperty(mp.Key);
                if (prop == null)
                {					
					TraceWarn("macro", string.Format("control property '{0}' doesn't exist or aren't accessible (public)", mp.Key));
                    continue;
                }

                var tryConvert = mp.Value.TryConvertTo(prop.PropertyType);
                if (tryConvert.Success)
                {
                    try
                    {
                        prop.SetValue(control, tryConvert.Result, null);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WarnWithException<macro>(string.Format("Error adding property '{0}' with value '{1}'", mp.Key, mp.Value), ex);
                        if (GlobalSettings.DebugMode)
                        {
                            TraceWarn("macro.loadControlProperties", string.Format("Error adding property '{0}' with value '{1}'", mp.Key, mp.Value), ex);
                        }
                    }

                    if (GlobalSettings.DebugMode)
                    {
                        TraceInfo("macro.UpdateControlProperties", string.Format("Property added '{0}' with value '{1}'", mp.Key, mp.Value));
                    }
                }
                else
                {
                    LogHelper.Warn<macro>(string.Format("Error adding property '{0}' with value '{1}'", mp.Key, mp.Value));
                    if (GlobalSettings.DebugMode)
                    {
                        TraceWarn("macro.loadControlProperties", string.Format("Error adding property '{0}' with value '{1}'", mp.Key, mp.Value));
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
                                      StateHelper.GetContextValue<int>(MacrosAddedKey));

                TraceInfo(LoadUserControlKey, string.Format("Usercontrol added with id '{0}'", oControl.ID));

	            AddCurrentNodeToControl(oControl);
                UpdateControlProperties(oControl, model);
                return oControl;
            }
            catch (Exception e)
            {
				LogHelper.WarnWithException<macro>(string.Format("Error creating usercontrol ({0})", fileName), true, e);
                throw;
            }
        }

        private static void AddCurrentNodeToControl(Control control)
        {
            var type = control.GetType();

            PropertyInfo currentNodeProperty = type.GetProperty("CurrentNode");
            if (currentNodeProperty != null && currentNodeProperty.CanWrite &&
                currentNodeProperty.PropertyType.IsAssignableFrom(typeof(Node)))
            {
                currentNodeProperty.SetValue(control, GetCurrentNode(), null);
            }
            currentNodeProperty = type.GetProperty("currentNode");
            if (currentNodeProperty != null && currentNodeProperty.CanWrite &&
                currentNodeProperty.PropertyType.IsAssignableFrom(typeof(Node)))
            {
                currentNodeProperty.SetValue(control, GetCurrentNode(), null);
            }
        }

        private static void TraceInfo(string category, string message, bool excludeProfiling = false)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write(category, message);

            //Trace out to profiling... doesn't actually profile, just for informational output.
            if (excludeProfiling == false)
            {
                using (ProfilerResolver.Current.Profiler.Step(string.Format("{0}", message)))
                {
                }
            }
        }

        private static void TraceWarn(string category, string message, bool excludeProfiling = false)
        {
            if (HttpContext.Current != null)
				HttpContext.Current.Trace.Warn(category, message);

            //Trace out to profiling... doesn't actually profile, just for informational output.
            if (excludeProfiling == false)
            {
                using (ProfilerResolver.Current.Profiler.Step(string.Format("Warning: {0}", message)))
                {
                }
            }
        }

        private static void TraceWarn(string category, string message, Exception ex, bool excludeProfiling = false)
		{
			if (HttpContext.Current != null)
				HttpContext.Current.Trace.Warn(category, message, ex);

            //Trace out to profiling... doesn't actually profile, just for informational output.
            if (excludeProfiling == false)
            {
                using (ProfilerResolver.Current.Profiler.Step(string.Format("{0}, Error: {1}", message, ex)))
                {
                }
            }
		}

        public static string renderMacroStartTag(Hashtable attributes, int pageId, Guid versionId)
        {
            string div = "<div ";

            IDictionaryEnumerator ide = attributes.GetEnumerator();
            while (ide.MoveNext())
            {
                div += string.Format("umb_{0}=\"{1}\" ", ide.Key, EncodeMacroAttribute((ide.Value ?? string.Empty).ToString()));
            }

            div += "ismacro=\"true\" onresizestart=\"return false;\" umbVersionId=\"" + versionId +
                   "\" umbPageid=\"" +
                   pageId +
                   "\" title=\"This is rendered content from macro\" class=\"umbMacroHolder\"><!-- startUmbMacro -->";

            return div;
        }

        private static string EncodeMacroAttribute(string attributeContents)
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

            if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
            {
                return "<span style='color: red'>Cannot render macro content in the rich text editor when the application is running in a Partial Trust environment</span>";
            }
            
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
                        retVal = ShowNoMacroContent(currentMacro);

                    // Release the HttpWebResponse Resource.
                    myHttpWebResponse.Close();
                }
                catch (Exception)
                {
                    retVal = ShowNoMacroContent(currentMacro);
                }
                finally
                {
                    // Release the HttpWebResponse Resource.
                    if (myHttpWebResponse != null)
                        myHttpWebResponse.Close();
                }

                return retVal.Replace("\n", string.Empty).Replace("\r", string.Empty);
            }

            return ShowNoMacroContent(currentMacro);
        }

        private static string ShowNoMacroContent(macro currentMacro)
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

        private static INode GetCurrentNode()
        {
            //Get the current content request

            IPublishedContent content;
            if (UmbracoContext.Current.IsFrontEndUmbracoRequest)
            {
                content = UmbracoContext.Current.PublishedContentRequest != null
                    ? UmbracoContext.Current.PublishedContentRequest.PublishedContent
                    : null;
            }
            else
            {
                var pageId = UmbracoContext.Current.PageId;
                content = pageId.HasValue ? UmbracoContext.Current.ContentCache.GetById(pageId.Value) : null;
            }
                    
            return content == null ? null : LegacyNodeHelper.ConvertToNode(content);
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

    /// <summary>
    /// Event arguments used for the MacroRendering event
    /// </summary>
    public class MacroRenderingEventArgs : EventArgs
    {
        public Hashtable PageElements { get; private set; }
        public int PageId { get; private set; }

        public MacroRenderingEventArgs(Hashtable pageElements, int pageId)
        {
            PageElements = pageElements;
            PageId = pageId;
        }
    }

}