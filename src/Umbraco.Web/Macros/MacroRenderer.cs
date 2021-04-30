using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Macros
{
    internal class MacroRenderer : IMacroRenderer
    {
        private readonly IProfilingLogger _plogger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentSection _contentSection;
        private readonly ILocalizedTextService _textService;
        private readonly AppCaches _appCaches;
        private readonly IMacroService _macroService;

        public MacroRenderer(IProfilingLogger plogger, IUmbracoContextAccessor umbracoContextAccessor, IContentSection contentSection, ILocalizedTextService textService, AppCaches appCaches, IMacroService macroService)
        {
            _plogger = plogger ?? throw new ArgumentNullException(nameof(plogger));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _contentSection = contentSection ?? throw new ArgumentNullException(nameof(contentSection));
            _textService = textService;
            _appCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
            _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
        }

        #region MacroContent cache

        // gets this macro content cache identifier
        private string GetContentCacheIdentifier(MacroModel model, int pageId, string cultureName)
        {
            var id = new StringBuilder();

            var alias = model.Alias;
            id.AppendFormat("{0}-", alias);
            //always add current culture to the key to allow variants to have different cache results
            if (!string.IsNullOrEmpty(cultureName))
            {
                // are there any unusual culture formats we'd need to handle?
                id.AppendFormat("{0}-", cultureName);
            }

            if (model.CacheByPage)
                id.AppendFormat("{0}-", pageId);

            if (model.CacheByMember)
            {
                object key = 0;

                if (_umbracoContextAccessor.UmbracoContext.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                {
                    //ugh, membershipproviders :(
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

        // gets this macro content from the cache
        // ensuring that it is appropriate to use the cache
        private MacroContent GetMacroContentFromCache(MacroModel model)
        {
            // only if cache is enabled
            if (_umbracoContextAccessor.UmbracoContext.InPreviewMode || model.CacheDuration <= 0) return null;

            var cache = _appCaches.RuntimeCache;
            var macroContent = cache.GetCacheItem<MacroContent>(CacheKeys.MacroContentCacheKey + model.CacheIdentifier);

            if (macroContent == null) return null;

            _plogger.Debug<MacroRenderer, string>("Macro content loaded from cache '{MacroCacheId}'", model.CacheIdentifier);

            // ensure that the source has not changed
            // note: does not handle dependencies, and never has
            var macroSource = GetMacroFile(model); // null if macro is not file-based
            if (macroSource != null)
            {
                if (macroSource.Exists == false)
                {
                    _plogger.Debug<MacroRenderer>("Macro source does not exist anymore, ignore cache.");
                    return null;
                }

                if (macroContent.Date < macroSource.LastWriteTime)
                {
                    _plogger.Debug<MacroRenderer>("Macro source has changed, ignore cache.");
                    return null;
                }
            }

            // this is legacy and I'm not sure what exactly it is supposed to do
            if (macroContent.Control != null)
                macroContent.Control.ID = macroContent.ControlId;

            return macroContent;
        }

        // stores macro content into the cache
        private void AddMacroContentToCache(MacroModel model, MacroContent macroContent)
        {
            // only if cache is enabled
            if (_umbracoContextAccessor.UmbracoContext.InPreviewMode || model.CacheDuration <= 0) return;

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

            // this is legacy and I'm not sure what exactly it is supposed to do
            if (macroContent.Control != null)
                macroContent.ControlId = macroContent.Control.ID;

            // remember when we cache the content
            macroContent.Date = DateTime.Now;

            var cache = _appCaches.RuntimeCache;
            cache.Insert(
                CacheKeys.MacroContentCacheKey + model.CacheIdentifier,
                () => macroContent,
                new TimeSpan(0, 0, model.CacheDuration),
                priority: CacheItemPriority.NotRemovable
                );

            _plogger.Debug<MacroRenderer, string>("Macro content saved to cache '{MacroCacheId}'", model.CacheIdentifier);
        }

        // gets the macro source file name
        // null if the macro is not file-based
        internal static string GetMacroFileName(MacroModel model)
        {
            string filename;

            switch (model.MacroType)
            {
                case MacroTypes.PartialView:
                    filename = model.MacroSource; // partial views are saved with their full virtual path
                    break;
                default:
                    // not file-based, or not supported
                    filename = null;
                    break;
            }

            return filename;
        }

        // gets the macro source file
        // null if macro is not file-based
        private static FileInfo GetMacroFile(MacroModel model)
        {
            var filename = GetMacroFileName(model);
            if (filename == null) return null;

            var mapped = IOHelper.MapPath(filename);
            if (mapped == null) return null;

            var file = new FileInfo(mapped);
            return file.Exists ? file : null;
        }

        // updates the model properties values according to the attributes
        private static void UpdateMacroModelProperties(MacroModel model, IDictionary<string, object> macroParams)
        {
            foreach (var prop in model.Properties)
            {
                var key = prop.Key.ToLowerInvariant();
                prop.Value = macroParams != null && macroParams.ContainsKey(key)
                    ? macroParams[key]?.ToString() ?? string.Empty
                    : string.Empty;
            }
        }
        #endregion

        #region Render/Execute

        public MacroContent Render(string macroAlias, IPublishedContent content, IDictionary<string, object> macroParams)
        {
            var m = _appCaches.RuntimeCache.GetCacheItem(CacheKeys.MacroFromAliasCacheKey + macroAlias, () => _macroService.GetByAlias(macroAlias));

            if (m == null)
                throw new InvalidOperationException("No macro found by alias " + macroAlias);

            var page = new PublishedContentHashtableConverter(content);

            var macro = new MacroModel(m);

            UpdateMacroModelProperties(macro, macroParams);
            return Render(macro, content, page.Elements);
        }

        private MacroContent Render(MacroModel macro, IPublishedContent content, IDictionary pageElements)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var macroInfo = $"Render Macro: {macro.Name}, type: {macro.MacroType}, cache: {macro.CacheDuration}";
            using (_plogger.DebugDuration<MacroRenderer>(macroInfo, "Rendered Macro."))
            {
                // parse macro parameters ie replace the special [#key], [$key], etc. syntaxes
                foreach (var prop in macro.Properties)
                    prop.Value = ParseAttribute(pageElements, prop.Value);

                var cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                macro.CacheIdentifier = GetContentCacheIdentifier(macro, content.Id, cultureName);

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
                var attempt = ExecuteMacroOfType(macro, content);

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
                _plogger.Warn<MacroRenderer, string>(e, "Failed {MsgIn}", msgIn);

                var macroErrorEventArgs = new MacroErrorEventArgs
                {
                    Name = macro.Name,
                    Alias = macro.Alias,
                    MacroSource = macro.MacroSource,
                    Exception = e,
                    Behaviour = _contentSection.MacroErrorBehaviour
                };

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
        private Attempt<MacroContent> ExecuteMacroOfType(MacroModel model, IPublishedContent content)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // ensure that we are running against a published node (ie available in XML)
            // that may not be the case if the macro is embedded in a RTE of an unpublished document

            if (content == null)
                return Attempt.Fail(new MacroContent { Text = "[macro failed (no content)]" });

            var textService = _textService;

            switch (model.MacroType)
            {
                case MacroTypes.PartialView:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Executing PartialView: MacroSource=\"{model.MacroSource}\".",
                        "Executed PartialView.",
                        () => ExecutePartialView(model, content),
                        () => textService.Localize("errors", "macroErrorLoadingPartialView", new[] { model.MacroSource }));

                default:
                    return ExecuteMacroWithErrorWrapper(model,
                        $"Execute macro with unsupported type \"{model.MacroType}\".",
                        "Executed.",
                        () => { throw new Exception("Unsupported macro type."); },
                        () => textService.Localize("errors", "macroErrorUnsupportedType"));
            }
        }


        #endregion

        #region Execute engines

        /// <summary>
        /// Renders a PartialView Macro.
        /// </summary>
        /// <returns>The text output of the macro execution.</returns>
        private MacroContent ExecutePartialView(MacroModel macro, IPublishedContent content)
        {
            var engine = new PartialViewMacroEngine();
            return engine.Execute(macro, content);
        }

        #endregion

        #region Execution helpers

        // parses attribute value looking for [@requestKey], [%sessionKey], [#pageElement], [$recursiveValue]
        // supports fallbacks eg "[@requestKey],[%sessionKey],1234"
        private string ParseAttribute(IDictionary pageElements, string attributeValue)
        {
            if (pageElements == null) throw new ArgumentNullException(nameof(pageElements));

            // check for potential querystring/cookie variables
            attributeValue = attributeValue.Trim();
            if (attributeValue.StartsWith("[") == false)
                return attributeValue;

            var tokens = attributeValue.Split(Constants.CharArrays.Comma).Select(x => x.Trim()).ToArray();

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

            var context = _umbracoContextAccessor.UmbracoContext.HttpContext;

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

        private string ParseAttributeOnParents(IDictionary pageElements, string name)
        {
            if (pageElements == null) throw new ArgumentNullException(nameof(pageElements));
            // this was, and still is, an ugly piece of nonsense

            var value = string.Empty;
            var cache = _umbracoContextAccessor.UmbracoContext.Content;

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

    }

}
