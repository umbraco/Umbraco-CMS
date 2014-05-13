using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using umbraco;
using Umbraco.Web.WebApi;
using System;
using System.Net.Http.Headers;
using System.Web;
using System.IO;

namespace Umbraco.Web.Editors
{
    
    public class TuningController : UmbracoApiController
    {

        static string basePath = HttpContext.Current.Server.MapPath(@"\Umbraco\assets\less\");
        static string resultCssPath = HttpContext.Current.Server.MapPath(@"\Css\tuning.style.css");
        static string tuningStylePath = basePath + @"tuning.style.less";
        static string tuningParametersPath = basePath + @"tuning.lessParameters.less";

        [HttpGet]
        public HttpResponseMessage GetGoogleFont()
        {

            // Google Web Font API Key
            var APIKey = "AIzaSyDx7Y58UckkgiETJ_riiTcj7gr_zeCapw4";

            // Google Web Font JSON URL
            var googleWebFontAPIURL = string.Format("https://www.googleapis.com/webfonts/v1/webfonts?key={0}", APIKey);

            var response = "{}";
            using (var client = new System.Net.WebClient())
            {
                response = client.DownloadString(new Uri(googleWebFontAPIURL));
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(response)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage GetLessParameters()
        {

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            if (!File.Exists(tuningParametersPath))
                File.Create(tuningParametersPath);

            IList<string> parameters = new List<string>();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningParametersPath))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.Contains("@import"))
                        parameters.Add("\"" + line.Replace(":", "\":\"").Replace(";", "\"").Replace("@", "").Replace(";", ""));
                }
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{" + String.Join(",", parameters) + "}")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpPost]
        public HttpResponseMessage PostLessParameters()
        {

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            if (!File.Exists(tuningParametersPath))
                File.Create(tuningParametersPath);

            if (!File.Exists(resultCssPath))
                File.Create(resultCssPath);

            var result = HttpContext.Current.Request["result"];

            // Update less parameter file
            string gaImportList = string.Empty;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(tuningParametersPath))
            {
                foreach (string parameters in result.Trim().Split(';'))
                {
                    if (!string.IsNullOrEmpty(parameters))
                    {
                        file.WriteLine((parameters + ";").Replace(":;", ":'';"));
                    }
                    if (parameters.IndexOf("@import") >= 0)
                    {
                        // Hack for ClientDependency
                        gaImportList = gaImportList + parameters.Replace("@import", "@IMPORT") + ";";
                    }
                }
            }

            // Read Tuning style file
            string tuningStyleString = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningStylePath))
            {
                tuningStyleString = sr.ReadToEnd();
            }

            // Compile the Less file
            string compiledStyle = GetCssFromLessString(tuningStyleString, false, true, true);

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resultCssPath))
            {
                file.Write(gaImportList + compiledStyle);
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("ok")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        private static string GetCssFromLessString(string css, Boolean cacheEnabled, Boolean minifyOutput, Boolean disableVariableRedefines)
        {
            var config = dotless.Core.configuration.DotlessConfiguration.GetDefaultWeb();
            config.DisableVariableRedefines = disableVariableRedefines;
            config.CacheEnabled = cacheEnabled;
            config.MinifyOutput = minifyOutput;
            return dotless.Core.LessWeb.Parse(css, config);
        }

    }

}