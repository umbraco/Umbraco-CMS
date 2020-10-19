using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.Models
{
    public class ContentSettings
    {
        private const string DefaultPreviewBadge =
            @"<div id=""umbracoPreviewBadge"" class=""umbraco-preview-badge""><span class=""umbraco-preview-badge__header"">Preview mode</span><a href=""{0}/preview/end?redir={1}"" class=""umbraco-preview-badge__end""><svg viewBox=""0 0 100 100"" xmlns=""http://www.w3.org/2000/svg""><title>Click to end</title><path d=""M5273.1 2400.1v-2c0-2.8-5-4-9.7-4s-9.7 1.3-9.7 4v2a7 7 0 002 4.9l5 4.9c.3.3.4.6.4 1v6.4c0 .4.2.7.6.8l2.9.9c.5.1 1-.2 1-.8v-7.2c0-.4.2-.7.4-1l5.1-5a7 7 0 002-4.9zm-9.7-.1c-4.8 0-7.4-1.3-7.5-1.8.1-.5 2.7-1.8 7.5-1.8s7.3 1.3 7.5 1.8c-.2.5-2.7 1.8-7.5 1.8z""/><path d=""M5268.4 2410.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1h-4.3zM5272.7 2413.7h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1zM5272.7 2417h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1 0-.5-.4-1-1-1z""/><path d=""M78.2 13l-8.7 11.7a32.5 32.5 0 11-51.9 25.8c0-10.3 4.7-19.7 12.9-25.8L21.8 13a47 47 0 1056.4 0z""/><path d=""M42.7 2.5h14.6v49.4H42.7z""/></svg></a></div><style type=""text/css"">.umbraco-preview-badge {{position: absolute;top: 1em;right: 1em;display: inline-flex;background: #1b264f;color: #fff;padding: 1em;font-size: 12px;z-index: 99999999;justify-content: center;align-items: center;box-shadow: 0 10px 50px rgba(0, 0, 0, .1), 0 6px 20px rgba(0, 0, 0, .16);line-height: 1;}}.umbraco-preview-badge__header {{font-weight: bold;}}.umbraco-preview-badge__end {{width: 3em;padding: 1em;margin: -1em -1em -1em 2em;display: flex;flex-shrink: 0;align-items: center;align-self: stretch;}}.umbraco-preview-badge__end:hover,.umbraco-preview-badge__end:focus {{background: #f5c1bc;}}.umbraco-preview-badge__end svg {{fill: #fff;width:1em;}}</style>";

        public ContentNotificationSettings Notifications { get; set; } = new ContentNotificationSettings();

        public ContentImagingSettings Imaging { get; set; } = new ContentImagingSettings();

        public bool ResolveUrlsFromTextString { get; set; } = false;

        public ContentErrorPage[] Error404Collection { get; set; } = Array.Empty<ContentErrorPage>();

        public string PreviewBadge { get; set; } = DefaultPreviewBadge;

        public MacroErrorBehaviour MacroErrors { get; set; } = MacroErrorBehaviour.Inline;

        public IEnumerable<string> DisallowedUploadFiles { get; set; } = new[] { "ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd" };

        public IEnumerable<string> AllowedUploadFiles { get; set; } = Array.Empty<string>();

        public bool ShowDeprecatedPropertyEditors { get; set; } = false;

        public string LoginBackgroundImage { get; set; } = "assets/img/login.jpg";


    }
}
