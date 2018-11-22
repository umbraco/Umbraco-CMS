/*
 * $Id: GzipModule.cs 439 2007-11-26 13:26:10Z spocke $
 *
 * @author Moxiecode
 * @copyright Copyright © 2004-2007, Moxiecode Systems AB, All rights reserved.
 */

using System;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.IO;

namespace umbraco.presentation.plugins.tinymce3
{
    /// <summary>
    /// Description of HttpHandler.
    /// </summary>
    public class GzipModule : IModule
    {
        /// <summary></summary>
        /// <param name="context">Request context.</param>
        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            HttpServerUtility server = context.Server;
            GzipCompressor gzipCompressor = new GzipCompressor();
            System.Collections.Specialized.NameValueCollection configSection = new System.Collections.Specialized.NameValueCollection();
            string suffix = "_src", enc;
            string[] languages = request.QueryString["languages"].Split(',');
            bool supportsGzip;

            // Set response headers
            response.ContentType = "text/javascript";
            response.Charset = "UTF-8";
            response.Buffer = false;

            // UMBRACO: Populate the configsection if it's empty
            configSection.Add("GzipEnabled", "true");
            configSection.Add("InstallPath", IOHelper.ResolveUrl(SystemDirectories.UmbracoClient) + "/tinymce3");
            configSection.Add("GzipExpiresOffset", TimeSpan.FromDays(10).Ticks.ToString());

            // Setup cache
            response.Cache.SetExpires(DateTime.Now.AddTicks(long.Parse(configSection["GzipExpiresOffset"])));
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetValidUntilExpires(false);

            // Check if it supports gzip
            enc = Regex.Replace("" + request.Headers["Accept-Encoding"], @"\s+", "").ToLower();
            supportsGzip = enc.IndexOf("gzip") != -1 || request.Headers["---------------"] != null;
            enc = enc.IndexOf("x-gzip") != -1 ? "x-gzip" : "gzip";

            gzipCompressor.AddData("var tinyMCEPreInit = {base : '" + configSection["InstallPath"] + "', suffix : '" + suffix + "'};");

            // Add core
            gzipCompressor.AddFile(IOHelper.MapPath(configSection["InstallPath"] + "/tiny_mce" + suffix + ".js"));

            // Add core languages
            foreach (string lang in languages)
            {
                if (File.Exists(IOHelper.MapPath(configSection["InstallPath"] + "/langs/" + lang + ".js")))
                {
                    gzipCompressor.AddFile(IOHelper.MapPath(configSection["InstallPath"] + "/langs/" + lang + ".js"));
                }
                else
                {
                    LogHelper.Info<GzipModule>(string.Format("TinyMCE: Error loading language '{0}'. Please download language pack from tinymce.moxiecode.com", lang));
                }
            }
            // Add themes
            if (request.QueryString["themes"] != null)
            {
                foreach (string theme in request.QueryString["themes"].Split(','))
                {
                    gzipCompressor.AddFile(IOHelper.MapPath(configSection["InstallPath"] + "/themes/" + theme + "/editor_template" + suffix + ".js"));

                    // Add theme languages
                    foreach (string lang in languages)
                    {
                        string path = IOHelper.MapPath(configSection["InstallPath"] + "/themes/" + theme + "/langs/" + lang + ".js");

                        if (File.Exists(path))
                            gzipCompressor.AddFile(path);
                    }

                    gzipCompressor.AddData("tinymce.ThemeManager.urls['" + theme + "'] = tinymce.baseURL+'/themes/" + theme + "';");
                }
            }

            // Add plugins
            if (request.QueryString["plugins"] != null)
            {
                foreach (string plugin in request.QueryString["plugins"].Split(','))
                {
                    gzipCompressor.AddFile(IOHelper.MapPath(configSection["InstallPath"] + "/plugins/" + plugin + "/editor_plugin" + suffix + ".js"));

                    // Add plugin languages
                    foreach (string lang in languages)
                    {
                        string path = IOHelper.MapPath(configSection["InstallPath"] + "/plugins/" + plugin + "/langs/" + lang + ".js");

                        if (File.Exists(path))
                            gzipCompressor.AddFile(path);
                    }

                    gzipCompressor.AddData("tinymce.ThemeManager.urls['" + plugin + "'] = tinymce.baseURL+'/plugins/" + plugin + "';");
                }
            }

            // Add ASP.NET AJAX Script Notification 
            gzipCompressor.AddData("if (typeof (Sys) !== 'undefined') {\nSys.Application.notifyScriptLoaded();\n}");

            // Output compressed file
            gzipCompressor.NoCompression = !supportsGzip || (!String.IsNullOrEmpty(configSection["GzipNoCompression"]) && bool.Parse(configSection["GzipNoCompression"]));

            if (!gzipCompressor.NoCompression)
                response.AppendHeader("Content-Encoding", enc);


            gzipCompressor.DiskCache = !String.IsNullOrEmpty(configSection["GzipDiskCache"]) ? bool.Parse(configSection["GzipDiskCache"]) : false;
            gzipCompressor.CachePath = configSection["GzipCachePath"];

            gzipCompressor.Compress(response.OutputStream);
        }
    }
}
