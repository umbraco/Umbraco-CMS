using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Macros;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Macros
{
    public class MacroRenderer : IMacroRenderer
    {
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<MacroRenderer> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ContentSettings _contentSettings;
        private readonly ILocalizedTextService _textService;
        private readonly AppCaches _appCaches;
        private readonly IMacroService _macroService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICookieManager _cookieManager;
        private readonly IMemberUserKeyProvider _memberUserKeyProvider;
        private readonly ISessionManager _sessionManager;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MacroRenderer(
            IProfilingLogger profilingLogger ,
            ILogger<MacroRenderer> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IOptions<ContentSettings> contentSettings,
            ILocalizedTextService textService,
            AppCaches appCaches,
            IMacroService macroService,
            IHostingEnvironment hostingEnvironment,
            ICookieManager cookieManager,
            IMemberUserKeyProvider memberUserKeyProvider,
            ISessionManager sessionManager,
            IRequestAccessor requestAccessor,
             IHttpContextAccessor httpContextAccessor)
        {
            _profilingLogger = profilingLogger  ?? throw new ArgumentNullException(nameof(profilingLogger ));
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
            _textService = textService;
            _appCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
            _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _cookieManager = cookieManager;
            _memberUserKeyProvider = memberUserKeyProvider;
            _sessionManager = sessionManager;
            _requestAccessor = requestAccessor;
            _httpContextAccessor = httpContextAccessor;
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

                if (_umbracoContextAccessor.GetRequiredUmbracoContext().Security.IsAuthenticated())
                {
                    key = _memberUserKeyProvider.GetMemberProviderUserKey() ?? 0;
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

            _logger.LogDebug("Macro content loaded from cache '{MacroCacheId}'", model.CacheIdentifier);

            // ensure that the source has not changed
            // note: does not handle dependencies, and never has
            var macroSource = GetMacroFile(model); // null if macro is not file-based
            if (macroSource != null)
            {
                if (macroSource.Exists == false)
                {
                    _logger.LogDebug("Macro source does not exist anymore, ignore cache.");
                    return null;
                }

                if (macroContent.Date < macroSource.LastWriteTime)
                {
                    _logger.LogDebug("Macro source has changed, ignore cache.");
                    return null;
                }
            }

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
                var key = _memberUserKeyProvider.GetMemberProviderUserKey();
                if (key is null) return;
            }

            // remember when we cache the content
            macroContent.Date = DateTime.Now;

            var cache = _appCaches.RuntimeCache;
            cache.Insert(
                CacheKeys.MacroContentCacheKey + model.CacheIdentifier,
                () => macroContent,
                new TimeSpan(0, 0, model.CacheDuration)
                );

            _logger.LogDebug("Macro content saved to cache '{MacroCacheId}'", model.CacheIdentifier);
        }

        // gets the macro source file name
        // null if the macro is not file-based, or not supported
        internal static string GetMacroFileName(MacroModel model)
        {
            string filename = model.MacroSource; // partial views are saved with their full virtual path

            return string.IsNullOrEmpty(filename) ? null : filename;
        }

        // gets the macro source file
        // null if macro is not file-based
        private FileInfo GetMacroFile(MacroModel model)
        {
            var filename = GetMacroFileName(model);
            if (filename == null) return null;

            var mapped = _hostingEnvironment.MapPathContentRoot(filename);
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

            var macro = new MacroModel(m);

            UpdateMacroModelProperties(macro, macroParams);
            return Render(macro, content);
        }

        private MacroContent Render(MacroModel macro, IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var macroInfo = $"Render Macro: {macro.Name}, cache: {macro.CacheDuration}";
            using (_profilingLogger.DebugDuration<MacroRenderer>(macroInfo, "Rendered Macro."))
            {
                // parse macro parameters ie replace the special [#key], [$key], etc. syntaxes
                foreach (var prop in macro.Properties)
                    prop.Value = ParseAttribute(prop.Value);

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
            using (_profilingLogger.DebugDuration<MacroRenderer>(msgIn, msgOut))
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
                _logger.LogWarning(e, "Failed {MsgIn}", msgIn);

                var macroErrorEventArgs = new MacroErrorEventArgs
                {
                    Name = macro.Name,
                    Alias = macro.Alias,
                    MacroSource = macro.MacroSource,
                    Exception = e,
                    Behaviour = _contentSettings.MacroErrors
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

            return ExecuteMacroWithErrorWrapper(model,
                          $"Executing PartialView: MacroSource=\"{model.MacroSource}\".",
                          "Executed PartialView.",
                          () => ExecutePartialView(model, content),
                          () => textService.Localize("errors/macroErrorLoadingPartialView", new[] { model.MacroSource }));
        }


        #endregion

        #region Execute engines

        /// <summary>
        /// Renders a PartialView Macro.
        /// </summary>
        /// <returns>The text output of the macro execution.</returns>
        private MacroContent ExecutePartialView(MacroModel macro, IPublishedContent content)
        {
            var engine = new PartialViewMacroEngine(_umbracoContextAccessor, _httpContextAccessor, _hostingEnvironment);
            return engine.Execute(macro, content);
        }

        #endregion

        #region Execution helpers

        // parses attribute value looking for [@requestKey], [%sessionKey]
        // supports fallbacks eg "[@requestKey],[%sessionKey],1234"
        private string ParseAttribute(string attributeValue)
        {
            // check for potential querystring/cookie variables
            attributeValue = attributeValue.Trim();
            if (attributeValue.StartsWith("[") == false)
                return attributeValue;

            var tokens = attributeValue.Split(',').Select(x => x.Trim()).ToArray();

            // ensure we only process valid input ie each token must be [?x] and not eg a json array
            // like [1,2,3] which we don't want to parse - however the last one can be a literal, so
            // don't check on the last one which can be just anything - check all previous tokens

            char[] validTypes = { '@', '%' };
            if (tokens.Take(tokens.Length - 1).Any(x =>
                x.Length < 4 // ie "[?x]".Length - too short
                || x[0] != '[' // starts with [
                || x[x.Length - 1] != ']' // ends with ]
                || validTypes.Contains(x[1]) == false))
            {
                return attributeValue;
            }

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
                        attributeValue = _requestAccessor.GetRequestValue(name);
                        break;
                    case '%':
                        attributeValue = _sessionManager.GetSessionValue(name);
                        if (string.IsNullOrEmpty(attributeValue))
                            attributeValue = _cookieManager.GetCookieValue(name);
                        break;
                }

                attributeValue = attributeValue?.Trim();
                if (string.IsNullOrEmpty(attributeValue) == false)
                    break; // got a value, use it
            }

            return attributeValue;
        }

        #endregion

    }

}
