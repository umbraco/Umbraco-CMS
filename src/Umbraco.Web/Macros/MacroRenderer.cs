using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Macros
{
    public class MacroRenderer
    {
        private readonly ProfilingLogger _plogger;

        // todo: there are many more things that would need to be injected in here

        public MacroRenderer(ProfilingLogger plogger)
        {
            _plogger = plogger;
        }

        // probably can do better - just porting from v7
        public IList<Exception> Exceptions { get; } = new List<Exception>();

        #region MacroContent cache

        // gets this macro content cache identifier
        private static string GetContentCacheIdentifier(MacroModel model, int pageId)
        {
            var id = new StringBuilder();

            var alias = string.IsNullOrEmpty(model.ScriptCode)
                ? model.Alias
                : GenerateCacheKeyFromCode(model.ScriptCode);
            id.AppendFormat("{0}-", alias);

            if (model.CacheByPage)
                id.AppendFormat("{0}-", pageId);

            if (model.CacheByMember)
            {
                object key = 0;

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    var member = Core.Security.MembershipProviderExtensions.GetCurrentUser(provider);
                    key = member?.ProviderUserKey ?? 0;
                }

                id.AppendFormat("m{0}-", key);
            }

            foreach (var value in model.Properties.Select(x => x.Value))
                id.AppendFormat("{0}-", value.Length <= 255 ? value : value.Substring(0, 255));

            return id.ToString();
        }

        private static string GenerateCacheKeyFromCode(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));

            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var h in hash) sb.Append(h.ToString("X2"));
            return sb.ToString();
        }

        // gets this macro content from the cache
        // ensuring that it is appropriate to use the cache
        private static MacroContent GetMacroContentFromCache(MacroModel model)
        {
            // only if cache is enabled
            if (UmbracoContext.Current.InPreviewMode || model.CacheDuration <= 0) return null;

            var cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var macroContent = cache.GetCacheItem<MacroContent>(CacheKeys.MacroContentCacheKey + model.CacheIdentifier);

            if (macroContent == null) return null;

            LogHelper.Debug<MacroRenderer>("Macro content loaded from cache \"{0}\".", () => model.CacheIdentifier);

            // ensure that the source has not changed
            // note: does not handle dependencies, and never has
            var macroSource = GetMacroFile(model); // null if macro is not file-based
            if (macroSource != null)
            {
                if (macroSource.Exists == false)
                {
                    LogHelper.Debug<MacroRenderer>("Macro source does not exist anymore, ignore cache.");
                    return null;
                }

                if (macroContent.Date < macroSource.LastWriteTime)
                {
                    LogHelper.Debug<MacroRenderer>("Macro source has changed, ignore cache.");
                    return null;
                }
            }

            // this is legacy and I'm not sure what exactly it is supposed to do
            if (macroContent.Control != null)
                macroContent.Control.ID = macroContent.ControlId;

            return macroContent;
        }

        // stores macro content into the cache
        private static void AddMacroContentToCache(MacroModel model, MacroContent macroContent)
        {
            // only if cache is enabled
            if (UmbracoContext.Current.InPreviewMode || model.CacheDuration <= 0) return;

            // just make sure...
            if (macroContent == null) return;

            // do not cache if it should cache by member and there's not member
            if (model.CacheByMember)
            {
                var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                var member = Core.Security.MembershipProviderExtensions.GetCurrentUser(provider);
                var key = member?.ProviderUserKey;
                if (key == null) return;
            }

            // scripts and xslt can be cached as strings but not controls
            // as page events (Page_Load) wouldn't be hit -- yet caching
            // controls is a bad idea, it can lead to plenty of issues ?!
            // eg with IDs...

            // this is legacy and I'm not sure what exactly it is supposed to do
            if (macroContent.Control != null)
                macroContent.ControlId = macroContent.Control.ID;

            // remember when we cache the content
            macroContent.Date = DateTime.Now;

            var cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            cache.InsertCacheItem(
                CacheKeys.MacroContentCacheKey + model.CacheIdentifier,
                () => macroContent,
                new TimeSpan(0, 0, model.CacheDuration),
                priority: CacheItemPriority.NotRemovable
                );

            LogHelper.Debug<MacroRenderer>("Macro content saved to cache \"{0}\".", () => model.CacheIdentifier);
        }

        // gets the macro source file name
        // null if the macro is not file-based
        internal static string GetMacroFileName(MacroModel model)
        {
            string filename;

            switch (model.MacroType)
            {
                case MacroTypes.Xslt:
                    filename = SystemDirectories.Xslt.EnsureEndsWith('/') + model.Xslt;
                    break;
                //case MacroTypes.Script:
                //    // was "~/macroScripts/"
                //    filename = SystemDirectories.MacroScripts.EnsureEndsWith('/') + model.ScriptName;
                //    break;
                case MacroTypes.PartialView:
                    filename = model.ScriptName; //partial views are saved with their full virtual path
                    break;
                case MacroTypes.UserControl:
                    filename = model.TypeName; //user controls are saved with their full virtual path
                    break;
                //case MacroTypes.Script:
                //case MacroTypes.CustomControl:
                //case MacroTypes.Unknown:
                default:
                    // not file-based, or not supported
                    filename = null;
                    break;
            }

            return filename;
        }

        // gets the macro source file
        // null if macro is not file-based
        internal static FileInfo GetMacroFile(MacroModel model)
        {
            var filename = GetMacroFileName(model);
            if (filename == null) return null;

            var mapped = HostingEnvironment.MapPath(filename);
            if (mapped == null) return null;

            var file = new FileInfo(mapped);
            return file.Exists ? file : null;
        }

        #endregion

        #region MacroModel properties

        // updates the model properties values according to the attributes
        private static void UpdateMacroModelProperties(MacroModel model, Hashtable attributes)
        {
            foreach (var prop in model.Properties)
            {
                var key = prop.Key.ToLowerInvariant();
                prop.Value = attributes.ContainsKey(key)
                    ? attributes[key]?.ToString() ?? string.Empty
                    : string.Empty;
            }
        }

        // generates the model properties according to the attributes
        public static void GenerateMacroModelPropertiesFromAttributes(MacroModel model, Hashtable attributes)
        {
            foreach (string key in attributes.Keys)
                model.Properties.Add(new MacroPropertyModel(key, attributes[key].ToString()));
        }

        #endregion

        #region Render/Execute

        // still, this is ugly. The macro should have a Content property
        // referring to IPublishedContent we're rendering the macro against,
        // this is all soooo convoluted ;-(

        public MacroContent Render(MacroModel macro, Hashtable pageElements, int pageId, Hashtable attributes)
        {
            UpdateMacroModelProperties(macro, attributes);
            return Render(macro, pageElements, pageId);
        }

        public MacroContent Render(MacroModel macro, Hashtable pageElements, int pageId)
        {
            // trigger MacroRendering event so that the model can be manipulated before rendering
            OnMacroRendering(new MacroRenderingEventArgs(pageElements, pageId));

            var macroInfo = $"Render Macro: {macro.Name}, type: {macro.MacroType}, cache: {macro.CacheDuration}";
            using (_plogger.DebugDuration<MacroRenderer>(macroInfo, "Rendered Macro."))
            {
                // parse macro parameters ie replace the special [#key], [$key], etc. syntaxes
                foreach (var prop in macro.Properties)
                    prop.Value = ParseAttribute(pageElements, prop.Value);

                macro.CacheIdentifier = GetContentCacheIdentifier(macro, pageId);

                // get the macro from cache if it is there
                var macroContent = GetMacroContentFromCache(macro);

                // macroContent.IsEmpty may be true, meaning the macro produces no output,
                // but still can be cached because its execution did not trigger any error.
                // so we need to actually render, only if macroContent is null
                if (macroContent != null)
                    return macroContent;

                // this will take care of errors
                // it may throw, if we actually want to throw, so better not
                // catch anything here and let the exception be thrown
                var attempt = ExecuteMacroOfType(macro);

                // by convention ExecuteMacroByType must either throw or return a result
                // just check to avoid internal errors
                macroContent = attempt.Result;
                if (macroContent == null)
                    throw new Exception("Internal error, ExecuteMacroOfType returned no content.");

                // add to cache if render is successful
                // content may be empty but that's not an issue
                if (attempt.Success)
                {
                    // write to cache (if appropriate)
                    AddMacroContentToCache(macro, macroContent);
                }

                return macroContent;
            }
        }

        /// <summary>
        /// Executes a macro of a given type.
        /// </summary>
        private Attempt<MacroContent> ExecuteMacroWithErrorWrapper(MacroModel macro, string msgIn, string msgOut, Func<MacroContent> getMacroContent, Func<string> msgErr)
        {
            using (_plogger.DebugDuration<MacroRenderer>(msgIn, msgOut))
            {
                return ExecuteProfileMacroWithErrorWrapper(macro, msgIn, getMacroContent, msgErr);
            }
        }

        /// <summary>
        /// Executes a macro of a given type.
        /// </summary>
        private Attempt<MacroContent> ExecuteProfileMacroWithErrorWrapper(MacroModel macro, string msgIn, Func<MacroContent> getMacroContent, Func<string> msgErr)
        {
            try
            {
                return Attempt.Succeed(getMacroContent());
            }
            catch (Exception e)
            {
                Exceptions.Add(e);

                _plogger.Logger.WarnWithException<MacroRenderer>("Failed " + msgIn, e);

                var macroErrorEventArgs = new MacroErrorEventArgs
                {
                    Name = macro.Name,
                    Alias = macro.Alias,
                    ItemKey = macro.ScriptName,
                    Exception = e,
                    Behaviour = UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour
                };

                OnError(macroErrorEventArgs);

                switch (macroErrorEventArgs.Behaviour)
                {
                    case MacroErrorBehaviour.Inline:
                        // do not throw, eat the exception, display the trace error message
                        return Attempt.Fail(new MacroContent { Text = msgErr() }, e);
                    case MacroErrorBehaviour.Silent:
                        // do not throw, eat the exception, do not display anything
                        return Attempt.Fail(new MacroContent { Text = string.Empty }, e);
                    case MacroErrorBehaviour.Content:
                        // do not throw, eat the exception, display the custom content
                        return Attempt.Fail(new MacroContent { Text = macroErrorEventArgs.Html ?? string.Empty }, e);
                    //case MacroErrorBehaviour.Throw:
                    default:
                        // see http://issues.umbraco.org/issue/U4-497 at the end
                        // throw the original exception
                        throw;
                }
            }
        }

        /// <summary>
        /// Executes a macro.
        /// </summary>
        /// <remarks>Returns an attempt that is successful if the macro ran successfully. If the macro failed
        /// to run properly, the attempt fails, though it may contain a content. But for instance that content
        /// should not be cached. In that case the attempt may also contain an exception.</remarks>
        private Attempt<MacroContent> ExecuteMacroOfType(MacroModel model)
        {
            // ensure that we are running against a published node (ie available in XML)
            // that may not be the case if the macro is embedded in a RTE of an unpublished document

            if (UmbracoContext.Current.PublishedContentRequest == null
                || UmbracoContext.Current.PublishedContentRequest.HasPublishedContent == false)
                return Attempt.Fail(new MacroContent { Text = "[macro]" });

            var textService = ApplicationContext.Current.Services.TextService;

            switch (model.MacroType)
            {
                case MacroTypes.PartialView:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Executing PartialView: TypeName=\"{model.TypeName}\", ScriptName=\"{model.ScriptName}\".",
                        "Executed PartialView.",
                        () => ExecutePartialView(model),
                        () => textService.Localize("errors/macroErrorLoadingPartialView", new[] { model.ScriptName }));

                //case MacroTypes.Script:
                //    return ExecuteMacroWithErrorWrapper(model,
                //        "Executing Script: " + (string.IsNullOrWhiteSpace(model.ScriptCode)
                //            ? "ScriptName=\"" + model.ScriptName + "\""
                //            : "Inline, Language=\"" + model.ScriptLanguage + "\""),
                //        "Executed Script.",
                //        () => ExecuteScript(model),
                //        () => textService.Localize("errors/macroErrorLoadingMacroEngineScript", new[] { model.ScriptName }));

                case MacroTypes.Xslt:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Executing Xslt: TypeName=\"{model.TypeName}\", ScriptName=\"{model.Xslt}\".",
                        "Executed Xslt.",
                        () => ExecuteXslt(model, _plogger),
                        // cannot diff. between reading & parsing... bah
                        () => textService.Localize("errors/macroErrorParsingXSLTFile", new[] { model.Xslt }));

                case MacroTypes.UserControl:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Loading UserControl: TypeName=\"{model.TypeName}\".",
                        "Loaded UserControl.",
                        () => ExecuteUserControl(model),
                        () => textService.Localize("errors/macroErrorLoadingUsercontrol", new[] { model.TypeName }));

                //case MacroTypes.Script:
                default:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Execute macro with unsupported type \"{model.MacroType}\".",
                        "Executed.",
                        () => { throw new Exception("Unsupported macro type."); },
                        () => textService.Localize("errors/macroErrorUnsupportedType"));
            }
        }

        // raised when a macro triggers an error
        public static event TypedEventHandler<MacroRenderer, MacroErrorEventArgs> Error;

        protected void OnError(MacroErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        // raised before the macro renders, allowing devs to modify it
        public static event TypedEventHandler<MacroRenderer, MacroRenderingEventArgs> MacroRendering;

        protected void OnMacroRendering(MacroRenderingEventArgs e)
        {
            MacroRendering?.Invoke(this, e);
        }

        #endregion

        #region Execute engines

        /// <summary>
        /// Renders a PartialView Macro.
        /// </summary>
        /// <returns>The text output of the macro execution.</returns>
        private static MacroContent ExecutePartialView(MacroModel macro)
        {
            var engine = new PartialViewMacroEngine();
            var content = UmbracoContext.Current.PublishedContentRequest.PublishedContent;
            return engine.Execute(macro, content);
        }

        /// <summary>
        /// Renders an Xslt Macro.
        /// </summary>
        /// <returns>The text output of the macro execution.</returns>
        public static MacroContent ExecuteXslt(MacroModel macro, ProfilingLogger plogger)
        {
            var engine = new XsltMacroEngine(plogger);
            return engine.Execute(macro);
        }

        public static MacroContent ExecuteUserControl(MacroModel macro)
        {
            // add tilde for v4 defined macros
            if (string.IsNullOrEmpty(macro.TypeName) == false
                && macro.TypeName.StartsWith("~") == false)
                macro.TypeName = "~/" + macro.TypeName;

            var engine = new UserControlMacroEngine();
            return engine.Execute(macro);
        }

        #endregion

        #region Execution helpers

        // parses attribute value looking for [@requestKey], [%sessionKey], [#pageElement], [$recursiveValue]
        // supports fallbacks eg "[@requestKey],[%sessionKey],1234"
        public static string ParseAttribute(IDictionary pageElements, string attributeValue)
        {

            // check for potential querystring/cookie variables
            attributeValue = attributeValue.Trim();
            if (attributeValue.StartsWith("[") == false)
                return attributeValue;

            var tokens = attributeValue.Split(',').Select(x => x.Trim()).ToArray();

            // ensure we only process valid input ie each token must be [?x] and not eg a json array
            // like [1,2,3] which we don't want to parse - however the last one can be a literal, so
            // don't check on the last one which can be just anything - check all previous tokens

            char[] validTypes = { '@', '%', '#', '$' };
            if (tokens.Take(tokens.Length - 1).Any(x =>
                x.Length < 4 // ie "[?x]".Length - too short
                || x[0] != '[' // starts with [
                || x[x.Length - 1] != ']' // ends with ]
                || validTypes.Contains(x[1]) == false))
            {
                return attributeValue;
            }

            var context = HttpContext.Current;

            foreach (var token in tokens)
            {
                var isToken = token.Length > 4 && token[0] == '[' && token[token.Length - 1] == ']' && validTypes.Contains(token[1]);

                if (isToken == false)
                {
                    // anything that is not a token is a value, use it
                    attributeValue = token;
                    break;
                }

                var type = token[1];
                var name = token.Substring(2, token.Length - 3);

                switch (type)
                {
                    case '@':
                        attributeValue = context?.Request[name];
                        break;
                    case '%':
                        attributeValue = context?.Session[name]?.ToString();
                        if (string.IsNullOrEmpty(attributeValue))
                            attributeValue = context?.Request.GetCookieValue(name);
                        break;
                    case '#':
                        attributeValue = pageElements[name]?.ToString();
                        break;
                    case '$':
                        attributeValue = pageElements[name]?.ToString();
                        if (string.IsNullOrEmpty(attributeValue))
                            attributeValue = ParseAttributeOnParents(pageElements, name);
                        break;
                }

                attributeValue = attributeValue?.Trim();
                if (string.IsNullOrEmpty(attributeValue) == false)
                    break; // got a value, use it
            }

            return attributeValue;
        }

        private static string ParseAttributeOnParents(IDictionary pageElements, string name)
        {
            // this was, and still is, an ugly piece of nonsense

            var value = string.Empty;
            var cache = UmbracoContext.Current.ContentCache; // should be injected

            var splitpath = (string[])pageElements["splitpath"];
            for (var i = splitpath.Length - 1; i > 0; i--) // at 0 we have root (-1)
            {
                var content = cache.GetById(int.Parse(splitpath[i]));
                if (content == null) continue;
                value = content.Value(name)?.ToString();
                if (string.IsNullOrEmpty(value) == false) break;
            }

            return value;
        }

        #endregion

        #region RTE macros

        public static string RenderMacroStartTag(Hashtable attributes, int pageId, Guid versionId)
        {
            var div = "<div ";

            var ide = attributes.GetEnumerator();
            while (ide.MoveNext())
            {
                div += $"umb_{ide.Key}=\"{EncodeMacroAttribute((ide.Value ?? String.Empty).ToString())}\" ";
            }

            div += $"ismacro=\"true\" onresizestart=\"return false;\" umbVersionId=\"{versionId}\" umbPageid=\"{pageId}\""
                + " title=\"This is rendered content from macro\" class=\"umbMacroHolder\"><!-- startUmbMacro -->";

            return div;
        }

        private static string EncodeMacroAttribute(string attributeContents)
        {
            // replace linebreaks
            attributeContents = attributeContents.Replace("\n", "\\n").Replace("\r", "\\r");

            // replace quotes
            attributeContents = attributeContents.Replace("\"", "&quot;");

            // replace tag start/ends
            attributeContents = attributeContents.Replace("<", "&lt;").Replace(">", "&gt;");

            return attributeContents;
        }

        public static string RenderMacroEndTag()
        {
            return "<!-- endUmbMacro --></div>";
        }

        private static readonly Regex HrefRegex = new Regex("href=\"([^\"]*)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static string GetRenderedMacro(int macroId, Hashtable elements, Hashtable attributes, int pageId, IMacroService macroService, ProfilingLogger plogger)
        {
            var m = macroService.GetById(macroId);
            if (m == null) return string.Empty;
            var model = new MacroModel(m);

            // get as text, will render the control if any
            var renderer = new MacroRenderer(plogger);
            var macroContent = renderer.Render(model, elements, pageId);
            var text = macroContent.GetAsText();

            // remove hrefs
            text = HrefRegex.Replace(text, match => "href=\"javascript:void(0)\"");

            return text;
        }

        public static string MacroContentByHttp(int pageId, Guid pageVersion, Hashtable attributes, IMacroService macroService)
        {
            // though... we only support FullTrust now?
            if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
                return "<span style='color: red'>Cannot render macro content in the rich text editor when the application is running in a Partial Trust environment</span>";

            var tempAlias = attributes["macroalias"]?.ToString() ?? attributes["macroAlias"].ToString();

            var m = macroService.GetByAlias(tempAlias);
            if (m == null) return string.Empty;
            var macro = new MacroModel(m);
            if (macro.RenderInEditor == false)
                return ShowNoMacroContent(macro);

            var querystring = $"umbPageId={pageId}&umbVersionId={pageVersion}";
            var ide = attributes.GetEnumerator();
            while (ide.MoveNext())
                querystring += $"&umb_{ide.Key}={HttpContext.Current.Server.UrlEncode((ide.Value ?? String.Empty).ToString())}";

            // create a new 'HttpWebRequest' object to the mentioned URL.
            var useSsl = GlobalSettings.UseSSL;
            var protocol = useSsl ? "https" : "http";
            var currentRequest = HttpContext.Current.Request;
            var serverVars = currentRequest.ServerVariables;
            var umbracoDir = IOHelper.ResolveUrl(SystemDirectories.Umbraco);
            var url = $"{protocol}://{serverVars["SERVER_NAME"]}:{serverVars["SERVER_PORT"]}{umbracoDir}/macroResultWrapper.aspx?{querystring}";

            var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            // allows for validation of SSL conversations (to bypass SSL errors in debug mode!)
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

            // propagate the user's context
            // TODO: this is the worst thing ever.
            // also will not work if people decide to put their own custom auth system in place.
            var inCookie = currentRequest.Cookies[UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName];
            if (inCookie == null) throw new NullReferenceException("No auth cookie found");
            var cookie = new Cookie(inCookie.Name, inCookie.Value, inCookie.Path, serverVars["SERVER_NAME"]);
            myHttpWebRequest.CookieContainer = new CookieContainer();
            myHttpWebRequest.CookieContainer.Add(cookie);

            // assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
            HttpWebResponse myHttpWebResponse = null;
            var text = string.Empty;
            try
            {
                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    var streamResponse = myHttpWebResponse.GetResponseStream();
                    if (streamResponse == null)
                        throw new Exception("Internal error, no response stream.");
                    var streamRead = new StreamReader(streamResponse);
                    var readBuff = new char[256];
                    var count = streamRead.Read(readBuff, 0, 256);
                    while (count > 0)
                    {
                        var outputData = new string(readBuff, 0, count);
                        text += outputData;
                        count = streamRead.Read(readBuff, 0, 256);
                    }

                    streamResponse.Close();
                    streamRead.Close();

                    // find the content of a form
                    const string grabStart = "<!-- grab start -->";
                    const string grabEnd = "<!-- grab end -->";

                    var grabStartPos = text.InvariantIndexOf(grabStart) + grabStart.Length;
                    var grabEndPos = text.InvariantIndexOf(grabEnd) - grabStartPos;
                    text = text.Substring(grabStartPos, grabEndPos);
                }
                else
                {
                    text = ShowNoMacroContent(macro);
                }
            }
            catch (Exception)
            {
                text = ShowNoMacroContent(macro);
            }
            finally
            {
                // release the HttpWebResponse Resource.
                myHttpWebResponse?.Close();
            }

            return text.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        private static string ShowNoMacroContent(MacroModel model)
        {
            var name = HttpUtility.HtmlEncode(model.Name); // safe
            return $"<span style=\"color: green\"><strong>{name}</strong><br />No macro content available for WYSIWYG editing</span>";
        }

        private static bool ValidateRemoteCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors policyErrors
            )
        {
            // allow any old dodgy certificate in debug mode
            return GlobalSettings.DebugMode || policyErrors == SslPolicyErrors.None;
        }

        #endregion
    }

    public class MacroRenderingEventArgs : EventArgs
    {
        public MacroRenderingEventArgs(Hashtable pageElements, int pageId)
        {
            PageElements = pageElements;
            PageId = pageId;
        }

        public int PageId { get; }

        public Hashtable PageElements { get; }
    }
}
