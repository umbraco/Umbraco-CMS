using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.Composing;
using Umbraco.Web.Templates;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// A macro engine using XSLT to execute.
    /// </summary>
    public class XsltMacroEngine
    {
        private readonly Func<HttpContextBase> _getHttpContext;
        private readonly Func<UmbracoContext> _getUmbracoContext;
        private readonly ProfilingLogger _plogger;

        public XsltMacroEngine(ProfilingLogger plogger)
        {
            _plogger = plogger;

            _getHttpContext = () =>
            {
                if (HttpContext.Current == null)
                    throw new InvalidOperationException("The Xslt Macro Engine cannot execute with a null HttpContext.Current reference");
                return new HttpContextWrapper(HttpContext.Current);
            };

            _getUmbracoContext = () =>
            {
                if (UmbracoContext.Current == null)
                    throw new InvalidOperationException("The Xslt Macro Engine cannot execute with a null UmbracoContext.Current reference");
                return UmbracoContext.Current;
            };
        }

        #region Execute Xslt

        // executes the macro, relying on GetXsltTransform
        // will pick XmlDocument or Navigator mode depending on the capabilities of the published caches
        public MacroContent Execute(MacroModel model)
        {
            var hasCode = string.IsNullOrWhiteSpace(model.ScriptCode) == false;
            var hasXslt = string.IsNullOrWhiteSpace(model.Xslt) == false;

            if (hasXslt == false && hasCode == false)
            {
                Current.Logger.Warn<XsltMacroEngine>("Xslt is empty");
                return MacroContent.Empty;
            }

            if (hasCode && model.ScriptLanguage.InvariantEquals("xslt") == false)
            {
                Current.Logger.Warn<XsltMacroEngine>("Unsupported script language \"" + model.ScriptLanguage + "\".");
                return MacroContent.Empty;
            }

            var msg = "Executing Xslt: " + (hasCode ? "Inline." : "Xslt=\"" + model.Xslt + "\".");
            using (_plogger.DebugDuration<XsltMacroEngine>(msg, "ExecutedXslt."))
            {
                // need these two here for error reporting
                MacroNavigator macroNavigator = null;
                XmlDocument macroXml = null;

                IXPathNavigable macroNavigable;
                IXPathNavigable contentNavigable;

                var xmlCache = UmbracoContext.Current.ContentCache as PublishedCache.XmlPublishedCache.PublishedContentCache;

                if (xmlCache == null)
                {
                    // a different cache
                    // inheriting from NavigableNavigator is required

                    var contentNavigator = UmbracoContext.Current.ContentCache.CreateNavigator() as NavigableNavigator;
                    var mediaNavigator = UmbracoContext.Current.MediaCache.CreateNavigator() as NavigableNavigator;

                    if (contentNavigator == null || mediaNavigator == null)
                        throw new Exception("Published caches XPathNavigator do not inherit from NavigableNavigator.");

                    var parameters = new List<MacroNavigator.MacroParameter>();
                    foreach (var prop in model.Properties)
                        AddMacroParameter(parameters, contentNavigator, mediaNavigator, prop.Key, prop.Type, prop.Value);

                    macroNavigable = macroNavigator = new MacroNavigator(parameters);
                    contentNavigable = UmbracoContext.Current.ContentCache;
                }
                else
                {
                    // the original XML cache
                    // render the macro on top of the Xml document

                    var umbracoXml = xmlCache.GetXml(UmbracoContext.Current.InPreviewMode);

                    macroXml = new XmlDocument();
                    macroXml.LoadXml("<macro/>");

                    foreach (var prop in model.Properties)
                        AddMacroXmlNode(umbracoXml, macroXml, prop.Key, prop.Type, prop.Value);

                    macroNavigable = macroXml;
                    contentNavigable = umbracoXml;
                }

                // this is somewhat ugly but eh...
                var httpContext = _getHttpContext();
                if (httpContext.Request.QueryString["umbDebug"] != null && GlobalSettings.DebugMode)
                {
                    var outerXml = macroXml?.OuterXml ?? macroNavigator.OuterXml;
                    var text = $"<div style=\"border: 2px solid green; padding: 5px;\"><b>Debug from {model.Name}<b><br />{HttpUtility.HtmlEncode(outerXml)}</div>";
                    return new MacroContent { Text = text };
                }

                // get the transform
                XslCompiledTransform transform;
                if (hasCode)
                {
                    try
                    {
                        using (var sreader = new StringReader(model.ScriptCode))
                        using (var xreader = new XmlTextReader(sreader))
                        {
                            transform = GetXsltTransform(xreader, GlobalSettings.DebugMode);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to parse inline Xslt.", e);
                    }
                }
                else
                {
                    try
                    {
                        transform = GetCachedXsltTransform(model.Xslt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Failed to read Xslt file \"{model.Xslt}\".", e);
                    }
                }

                using (_plogger.DebugDuration<XsltMacroEngine>("Performing transformation.", "Performed transformation."))
                {
                    try
                    {
                        var transformed = XsltTransform(_plogger, macroNavigable, contentNavigable, transform);
                        var text = TemplateUtilities.ResolveUrlsFromTextString(transformed);
                        return new MacroContent { Text = text };
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Failed to exec Xslt file \"{model.Xslt}\".", e);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the XSLT extension namespaces to the XSLT header using
        /// {0} as the container for the namespace references and
        /// {1} as the container for the exclude-result-prefixes
        /// </summary>
        /// <param name="xslt">The XSLT</param>
        /// <returns>The XSLT with {0} and {1} replaced.</returns>
        /// <remarks>This is done here because it needs the engine's XSLT extensions.</remarks>
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

        public static string TestXsltTransform(ProfilingLogger plogger, string xsltText, int currentPageId = -1)
        {
            IXPathNavigable macroNavigable;
            IXPathNavigable contentNavigable;

            var xmlCache = UmbracoContext.Current.ContentCache as PublishedCache.XmlPublishedCache.PublishedContentCache;

            var xslParameters = new Dictionary<string, object>();
            xslParameters["currentPage"] = UmbracoContext.Current.ContentCache
                .CreateNavigator()
                .Select(currentPageId > 0 ? ("//* [@id=" + currentPageId + "]") : "//* [@parentID=-1]");

            if (xmlCache == null)
            {
                // a different cache
                // inheriting from NavigableNavigator is required

                var contentNavigator = UmbracoContext.Current.ContentCache.CreateNavigator() as NavigableNavigator;
                if (contentNavigator == null)
                    throw new Exception("Published caches XPathNavigator do not inherit from NavigableNavigator.");

                var parameters = new List<MacroNavigator.MacroParameter>();
                macroNavigable = new MacroNavigator(parameters);
                contentNavigable = UmbracoContext.Current.ContentCache;
            }
            else
            {
                // the original XML cache
                // render the macro on top of the Xml document

                var umbracoXml = xmlCache.GetXml(UmbracoContext.Current.InPreviewMode);

                var macroXml = new XmlDocument();
                macroXml.LoadXml("<macro/>");

                macroNavigable = macroXml;
                contentNavigable = umbracoXml;
            }

            // for a test, do not try...catch
            // but let the exceptions be thrown

            XslCompiledTransform transform;
            using (var reader = new XmlTextReader(new StringReader(xsltText)))
            {
                transform = GetXsltTransform(reader, true);
            }
            var transformed = XsltTransform(plogger, macroNavigable, contentNavigable, transform, xslParameters);

            return transformed;
        }

        public static string ExecuteItemRenderer(ProfilingLogger plogger, XslCompiledTransform transform, string itemData)
        {
            IXPathNavigable macroNavigable;
            IXPathNavigable contentNavigable;

            var xmlCache = UmbracoContext.Current.ContentCache as PublishedCache.XmlPublishedCache.PublishedContentCache;

            if (xmlCache == null)
            {
                // a different cache
                // inheriting from NavigableNavigator is required

                var contentNavigator = UmbracoContext.Current.ContentCache.CreateNavigator() as NavigableNavigator;
                var mediaNavigator = UmbracoContext.Current.MediaCache.CreateNavigator() as NavigableNavigator;

                if (contentNavigator == null || mediaNavigator == null)
                    throw new Exception("Published caches XPathNavigator do not inherit from NavigableNavigator.");

                var parameters = new List<MacroNavigator.MacroParameter>();

                macroNavigable = new MacroNavigator(parameters);
                contentNavigable = UmbracoContext.Current.ContentCache;
            }
            else
            {
                // the original XML cache
                // render the macro on top of the Xml document

                var umbracoXml = xmlCache.GetXml(UmbracoContext.Current.InPreviewMode);

                var macroXml = new XmlDocument();
                macroXml.LoadXml("<macro/>");

                macroNavigable = macroXml;
                contentNavigable = umbracoXml;
            }

            var xslParameters = new Dictionary<string, object> { { "itemData", itemData } };
            return XsltTransform(plogger, macroNavigable, contentNavigable, transform, xslParameters);
        }

        #endregion

        #region XsltTransform

        // running on the XML cache, document mode
        // add parameters to the <macro> root node
        // note: contains legacy dirty code
        private static void AddMacroXmlNode(XmlDocument umbracoXml, XmlDocument macroXml,
            string macroPropertyAlias, string macroPropertyType, string macroPropertyValue)
        {
            var macroXmlNode = macroXml.CreateNode(XmlNodeType.Element, macroPropertyAlias, string.Empty);

            // if no value is passed, then use the current "pageID" as value
            var contentId = macroPropertyValue == string.Empty ? UmbracoContext.Current.PageId.ToString() : macroPropertyValue;

            Current.Logger.Info<XsltMacroEngine>($"Xslt node adding search start ({macroPropertyAlias},{macroPropertyValue})");

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
                    foreach (XmlNode n in currentNode.SelectNodes("*[@isDoc]"))
                        currentNode.RemoveChild(n);

                    macroXmlNode.AppendChild(currentNode);

                    break;

                case "contentSubs": // disable that one, it does not work anyway...
                    //x.LoadXml("<nodes/>");
                    //x.FirstChild.AppendChild(x.ImportNode(umbracoXml.GetElementById(contentId), true));
                    //macroXmlNode.InnerXml = TransformMacroXml(x, "macroGetSubs.xsl");
                    break;

                case "contentAll":
                    macroXmlNode.AppendChild(macroXml.ImportNode(umbracoXml.DocumentElement, true));
                    break;

                case "contentRandom":
                    XmlNode source = umbracoXml.GetElementById(contentId);
                    if (source != null)
                    {
                        var sourceList = source.SelectNodes("*[@isDoc]");
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
                            foreach (XmlNode n in node.SelectNodes("*[@isDoc]"))
                                node.RemoveChild(n);

                            macroXmlNode.AppendChild(node);
                        }
                        else
                            Current.Logger.Warn<XsltMacroEngine>("Error adding random node - parent (" + macroPropertyValue + ") doesn't have children!");
                    }
                    else
                        Current.Logger.Warn<XsltMacroEngine>("Error adding random node - parent (" + macroPropertyValue + ") doesn't exists!");
                    break;

                case "mediaCurrent":
                    if (string.IsNullOrEmpty(macroPropertyValue) == false)
                    {
                        //var c = new global::umbraco.cms.businesslogic.Content(int.Parse(macroPropertyValue));
                        //macroXmlNode.AppendChild(macroXml.ImportNode(c.ToXml(global::umbraco.content.Instance.XmlContent, false), true));
                        var nav = UmbracoContext.Current.MediaCache.CreateNodeNavigator(int.Parse(macroPropertyValue), false);
                        if (nav != null)
                            macroXmlNode.AppendChild(macroXml.ReadNode(nav.ReadSubtree()));
                    }
                    break;

                default:
                    macroXmlNode.InnerText = HttpUtility.HtmlDecode(macroPropertyValue);
                    break;
            }
            macroXml.FirstChild.AppendChild(macroXmlNode);
        }

        // running on a navigable cache, navigable mode
        // add parameters to the macro parameters collection
        private static void AddMacroParameter(ICollection<MacroNavigator.MacroParameter> parameters,
            NavigableNavigator contentNavigator, NavigableNavigator mediaNavigator,
            string macroPropertyAlias, string macroPropertyType, string macroPropertyValue)
        {
            // if no value is passed, then use the current "pageID" as value
            var contentId = macroPropertyValue == string.Empty ? UmbracoContext.Current.PageId.ToString() : macroPropertyValue;

            Current.Logger.Info<XsltMacroEngine>($"Xslt node adding search start ({macroPropertyAlias},{macroPropertyValue})");

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
                                nav = contentNavigator.CloneWithNewRoot(node.Id);
                                parameters.Add(new MacroNavigator.MacroParameter(macroPropertyAlias, nav, 0));
                            }
                            else
                                throw new InvalidOperationException("Iterator contains non-INavigableContent elements.");
                        }
                        else
                            Current.Logger.Warn<XsltMacroEngine>("Error adding random node - parent (" + macroPropertyValue + ") doesn't have children!");
                    }
                    else
                        Current.Logger.Warn<XsltMacroEngine>("Error adding random node - parent (" + macroPropertyValue + ") doesn't exists!");
                    break;

                case "mediaCurrent":
                    parameters.Add(new MacroNavigator.MacroParameter(
                        macroPropertyAlias,
                        mediaNavigator.CloneWithNewRoot(contentId), // null if not found - will be reported as empty
                        0));
                    break;

                default:
                    parameters.Add(new MacroNavigator.MacroParameter(macroPropertyAlias, HttpUtility.HtmlDecode(macroPropertyValue)));
                    break;
            }
        }

        // gets the result of the xslt transform
        private static string XsltTransform(ProfilingLogger plogger, IXPathNavigable macroNavigable, IXPathNavigable contentNavigable,
            XslCompiledTransform xslt, IDictionary<string, object> xslParameters = null)
        {
            TextWriter tw = new StringWriter();

            XsltArgumentList xslArgs;
            using (plogger.DebugDuration<XsltMacroEngine>("Adding Xslt extensions", "Added Xslt extensions"))
            {
                xslArgs = GetXsltArgumentListWithExtensions();
                var lib = new library();
                xslArgs.AddExtensionObject("urn:umbraco.library", lib);
            }

            // add parameters
            if (xslParameters == null || xslParameters.ContainsKey("currentPage") == false)
            {
                // note: "PageId" is a legacy stuff that might be != from what's in current PublishedContentRequest
                var currentPageId = UmbracoContext.Current.PageId;
                var current = contentNavigable.CreateNavigator().Select("//* [@id=" + currentPageId + "]");
                xslArgs.AddParam("currentPage", string.Empty, current);
            }
            if (xslParameters != null)
                foreach (var parameter in xslParameters)
                    xslArgs.AddParam(parameter.Key, string.Empty, parameter.Value);

            // transform
            using (plogger.DebugDuration<XsltMacroEngine>("Executing Xslt transform", "Executed Xslt transform"))
            {
                xslt.Transform(macroNavigable, xslArgs, tw);
            }

            return tw.ToString();
        }

        #endregion

        #region Manage transforms

        private static XslCompiledTransform GetCachedXsltTransform(string filename)
        {
            //TODO: SD: Do we really need to cache this??
            var filepath = IOHelper.MapPath(SystemDirectories.Xslt.EnsureEndsWith('/') + filename);
            return Current.ApplicationCache.GetCacheItem(
                CacheKeys.MacroXsltCacheKey + filename,
                CacheItemPriority.Default,
                new CacheDependency(filepath),
                () =>
                {
                    using (var xslReader = new XmlTextReader(filepath))
                    {
                        return GetXsltTransform(xslReader, GlobalSettings.DebugMode);
                    }
                });
        }

        public static XslCompiledTransform GetXsltTransform(XmlTextReader xslReader, bool debugMode)
        {
            var transform = new XslCompiledTransform(debugMode);
            var xslResolver = new XmlUrlResolver
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            xslReader.EntityHandling = EntityHandling.ExpandEntities;
            xslReader.DtdProcessing = DtdProcessing.Parse;

            try
            {
                transform.Load(xslReader, XsltSettings.TrustedXslt, xslResolver);
            }
            finally
            {
                xslReader.Close();
            }

            return transform;
        }

        #endregion

        #region Manage extensions

        /*
        private static readonly string XsltExtensionsConfig =
            IOHelper.MapPath(SystemDirectories.Config + "/xsltExtensions.config");

        private static readonly Func<CacheDependency> XsltExtensionsDependency =
            () => new CacheDependency(XsltExtensionsConfig);
        */

        // creates and return an Xslt argument list with all Xslt extensions.
        public static XsltArgumentList GetXsltArgumentListWithExtensions()
        {
            var xslArgs = new XsltArgumentList();

            foreach (var extension in GetXsltExtensions())
            {
                var extensionNamespace = "urn:" + extension.Key;
                xslArgs.AddExtensionObject(extensionNamespace, extension.Value);
                Current.Logger.Info<XsltMacroEngine>($"Extension added: {extensionNamespace}, {extension.Value.GetType().Name}");
            }

            return xslArgs;
        }

        /*
        // gets the collection of all XSLT extensions for macros
        // ie predefined, configured in the config file, and marked with the attribute
        public static Dictionary<string, object> GetCachedXsltExtensions()
        {
            // We could cache the extensions in a static variable but then the cache
            // would not be refreshed when the .config file is modified. An application
            // restart would be required. Better use the cache and add a dependency.

            // SD: The only reason the above statement might be true is because the xslt extension .config file is not a
            // real config file!! if it was, we wouldn't have this issue. Having these in a static variable would be preferred!
            //  If you modify a config file, the app restarts and thus all static variables are reset.
            //  Having this stuff in cache just adds to the gigantic amount of cache data and will cause more cache turnover to happen.

            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                "UmbracoXsltExtensions",
                CacheItemPriority.NotRemovable, // NH 4.7.1, Changing to NotRemovable
                null, // no refresh action
                XsltExtensionsDependency(), // depends on the .config file
                TimeSpan.FromDays(1), // expires in 1 day (?)
                GetXsltExtensions);
        }
        */

        // actually gets the collection of all XSLT extensions for macros
        // ie predefined, configured in the config file, and marked with the attribute
        public static Dictionary<string, object> GetXsltExtensions()
        {
            return Current.XsltExtensions
                .ToDictionary(x => x.Namespace, x => x.ExtensionObject);

            /*
            // initialize the collection
            // there is no "predefined" extensions anymore
            var extensions = new Dictionary<string, object>();

            // Load the XSLT extensions configuration
            var xsltExt = new XmlDocument();
            xsltExt.Load(XsltExtensionsConfig);

            // get the configured types
            var extensionsNode = xsltExt.SelectSingleNode("/XsltExtensions");
            if (extensionsNode != null)
                foreach (var attributes in extensionsNode.Cast<XmlNode>()
                    .Where(x => x.NodeType == XmlNodeType.Element)
                    .Select(x => x.Attributes))
                {
                    Debug.Assert(attributes["assembly"] != null, "Extension attribute 'assembly' not specified.");
                    Debug.Assert(attributes["type"] != null, "Extension attribute 'type' not specified.");
                    Debug.Assert(attributes["alias"] != null, "Extension attribute 'alias' not specified.");

                    // load the extension assembly
                    var extensionFile = IOHelper.MapPath(string.Format("{0}/{1}.dll",
                        SystemDirectories.Bin, attributes["assembly"].Value));

                    Assembly extensionAssembly;
                    try
                    {
                        extensionAssembly = Assembly.LoadFrom(extensionFile);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            String.Format("Could not load assembly {0} for XSLT extension {1}. Please check config/xsltExtensions.config.",
                                extensionFile, attributes["alias"].Value), ex);
                    }

                    // load the extension type
                    var extensionType = extensionAssembly.GetType(attributes["type"].Value);
                    if (extensionType == null)
                        throw new Exception(
                            String.Format("Could not load type {0} ({1}) for XSLT extension {2}. Please check config/xsltExtensions.config.",
                                attributes["type"].Value, extensionFile, attributes["alias"].Value));

                    // create an instance and add it to the extensions list
                    extensions.Add(attributes["alias"].Value, Activator.CreateInstance(extensionType));
                }

            // get types marked with XsltExtension attribute
            var foundExtensions = TypeLoader.Current.ResolveXsltExtensions();
            foreach (var xsltType in foundExtensions)
            {
                var attributes = xsltType.GetCustomAttributes<XsltExtensionAttribute>(true);
                var xsltTypeName = xsltType.FullName;
                foreach (var ns in attributes
                    .Select(attribute => string.IsNullOrEmpty(attribute.Namespace) ? attribute.Namespace : xsltTypeName))
                {
                    extensions.Add(ns, Activator.CreateInstance(xsltType));
                }
            }

            return extensions;
            */
        }

        #endregion
    }
}
