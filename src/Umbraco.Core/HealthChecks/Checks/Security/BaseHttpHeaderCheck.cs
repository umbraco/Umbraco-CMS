// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security
{
    /// <summary>
    /// Provides a base class for health checks of http header values.
    /// </summary>
    public abstract class BaseHttpHeaderCheck : HealthCheck
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILocalizedTextService _textService;
        private readonly string _header;
        private readonly string _localizedTextPrefix;
        private readonly bool _metaTagOptionAvailable;
        private static HttpClient? s_httpClient;

        [Obsolete("Use ctor without value.")]
        protected BaseHttpHeaderCheck(
            IHostingEnvironment hostingEnvironment,
            ILocalizedTextService textService,
            string header,
            string value,
            string localizedTextPrefix,
            bool metaTagOptionAvailable) :this(hostingEnvironment, textService, header, localizedTextPrefix, metaTagOptionAvailable)
        {

        }

        [Obsolete("Save ILocalizedTextService in a field on the super class instead of using this")]
        protected ILocalizedTextService LocalizedTextService => _textService;
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseHttpHeaderCheck"/> class.
        /// </summary>
        protected BaseHttpHeaderCheck(
            IHostingEnvironment hostingEnvironment,
            ILocalizedTextService textService,
            string header,
            string localizedTextPrefix,
            bool metaTagOptionAvailable)
        {
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _hostingEnvironment = hostingEnvironment;
            _header = header;
            _localizedTextPrefix = localizedTextPrefix;
            _metaTagOptionAvailable = metaTagOptionAvailable;
        }

        private static HttpClient HttpClient => s_httpClient ??= new HttpClient();

        /// <summary>
        /// Gets a link to an external read more page.
        /// </summary>
        protected abstract string ReadMoreLink { get; }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
            await Task.WhenAll(CheckForHeader());

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
            => throw new InvalidOperationException("HTTP Header action requested is either not executable or does not exist");

        /// <summary>
        /// The actual health check method.
        /// </summary>
        protected async Task<HealthCheckStatus> CheckForHeader()
        {
            string message;
            var success = false;

            // Access the site home page and check for the click-jack protection header or meta tag
            var url = _hostingEnvironment.ApplicationMainUrl?.GetLeftPart(UriPartial.Authority);

            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync(url);

                // Check first for header
                success = HasMatchingHeader(response.Headers.Select(x => x.Key));

                // If not found, and available, check for meta-tag
                if (success == false && _metaTagOptionAvailable)
                {
                    success = await DoMetaTagsContainKeyForHeader(response);
                }

                message = success
                    ? _textService.Localize($"healthcheck", $"{_localizedTextPrefix}CheckHeaderFound")
                    : _textService.Localize($"healthcheck", $"{_localizedTextPrefix}CheckHeaderNotFound");
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck","healthCheckInvalidUrl", new[] { url?.ToString(), ex.Message });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    ReadMoreLink = success ? null : ReadMoreLink
                };
        }

        private bool HasMatchingHeader(IEnumerable<string> headerKeys)
            => headerKeys.Contains(_header, StringComparer.InvariantCultureIgnoreCase);

        private async Task<bool> DoMetaTagsContainKeyForHeader(HttpResponseMessage response)
        {
            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                if (stream == null)
                {
                    return false;
                }

                using (var reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    Dictionary<string, string> metaTags = ParseMetaTags(html);
                    return HasMatchingHeader(metaTags.Keys);
                }
            }
        }

        private static Dictionary<string, string> ParseMetaTags(string html)
        {
            var regex = new Regex("<meta http-equiv=\"(.+?)\" content=\"(.+?)\"", RegexOptions.IgnoreCase);

            return regex.Matches(html)
                .Cast<Match>()
                .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        }
    }
}
