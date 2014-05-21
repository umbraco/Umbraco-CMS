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
        static string frontBasePath = HttpContext.Current.Server.MapPath(@"\Css\tuning\");
        static string resultCssPath = @"\Css\tuning\{0}.css";
        static string resultLessPath = @"\Css\tuning\{0}.less";
        static string tuningStylePath = basePath + @"tuning.defaultStyle.less";

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
        public HttpResponseMessage Load()
        {

            var tuningParametersPath = HttpContext.Current.Request["param"];

            if (string.IsNullOrEmpty(tuningParametersPath))
                tuningParametersPath = tuningStylePath;
            else
                tuningParametersPath = HttpContext.Current.Server.MapPath(tuningParametersPath);

            string paramBlock = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningParametersPath))
            {
                paramBlock = GetStyleBloque("lessParam", sr.ReadToEnd());
            }
            
            string[] paramLines = paramBlock.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            IList<string> parameters = new List<string>();          
            foreach (var line in paramLines)
            {
                if (!line.Contains("@import"))
                    parameters.Add("\"" + line.Replace(":", "\":\"").Replace(";", "\"").Replace("@", "").Replace(";", ""));
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{" + String.Join(",", parameters) + "}")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpPost]
        public HttpResponseMessage Save()
        {

            var result = HttpContext.Current.Request["result"];
            var pageId = HttpContext.Current.Request["pageId"];

            // Path to the new less and css files
            string newResultLessPath = HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId));
            string newResultCssPath = HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId));

            // Load default Less content
            string lessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningStylePath))
            {
                lessContent = sr.ReadToEnd();
            }

            // Create font directory
            if (!Directory.Exists(frontBasePath))
                Directory.CreateDirectory(frontBasePath);

            // Prepare parameters and gf block
            string newParamBlock = string.Empty;
            string newGfBlock = string.Empty;
            foreach (string parameters in result.Trim().Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameters.IndexOf("@import") < 0)
                {
                    newParamBlock += (parameters + ";").Replace(":;", ":'';") + Environment.NewLine;
                }
                else 
                {
                    newGfBlock += parameters + ";" + Environment.NewLine;
                }
            }
            lessContent = lessContent.Replace(GetStyleBloque("lessParam", lessContent), Environment.NewLine + newParamBlock);
            lessContent = lessContent.Replace(GetStyleBloque("gf", lessContent), Environment.NewLine + newGfBlock);

            // Save less file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultLessPath))
            {
                file.Write(lessContent);
            }

            // Compile the Less file
            string compiledStyle = GetCssFromLessString(lessContent, false, true, true).Replace("@import", "@IMPORT");

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultCssPath))
            {
                file.Write(compiledStyle);
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("ok")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpGet]
        public HttpResponseMessage Delete()
        {

            var pageId = HttpContext.Current.Request["pageId"];

            // Path to the less and css files
            string newResultLessPath = HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId));
            string newResultCssPath = HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId));

            // Delete all style file for this page
            System.IO.File.Delete(newResultLessPath);
            System.IO.File.Delete(newResultCssPath);

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

        private static string GetStyleBloque(string tag, string input)
        {
            string startTag = string.Format("/***start-{0}***/", tag);
            string endTag = string.Format("/***end-{0}***/", tag);

            int indexStartTag = input.IndexOf(startTag);
            int indexEndTag = input.IndexOf(endTag);

            if (indexStartTag >= 0 && indexEndTag >= 0)
                return input.Substring(indexStartTag, indexEndTag - indexStartTag).Replace(startTag, "").Replace(endTag, "");
            else
                return "";
        }

    }

}