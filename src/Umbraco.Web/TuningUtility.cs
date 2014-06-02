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
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Linq;
using System.Text.RegularExpressions;

namespace Umbraco.Web
{
    internal static class TuningUtility
    {

        internal static string assetBasePath = HttpContext.Current.Server.MapPath(@"\Umbraco\assets\less\");
        internal static string tuningDefaultLessPath = assetBasePath + @"tuning.defaultStyle.less";
        internal static string tuningGridRowStyle = assetBasePath + @"tuning.gridRowStyle.less";
        internal static string tuningStylePath = "/Css/tuning/";
        internal static string resultLessPath = tuningStylePath + @"{0}.less";
        internal static string resultCssPath = tuningStylePath + @"{0}.css";
        internal static string frontBasePath = HttpContext.Current.Server.MapPath(@"\Css\tuning\");

        // return the list of all grid's rows' guid for the current page 
        internal static String[] GetGridRows(int pageId) 
        {

            List<string> result = new List<string>();

            // Load current page
            var contentService = ApplicationContext.Current.Services.ContentService;
            IContent content = contentService.GetById(pageId);

            // Look after grid properies into the current page
            foreach (var property in content.Properties)
            {
                if (property.PropertyType.PropertyEditorAlias == "Umbraco.Grid" && property.Value != null && !string.IsNullOrEmpty(property.Value.ToString()))
                {
                    dynamic grid = Newtonsoft.Json.Linq.JObject.Parse(property.Value.ToString());
                    foreach (var column in grid.columns)
                    {
                        foreach (var row in column.rows)
                        {
                            if (row.uniqueId != null)
                                result.Add(row.uniqueId.Value);
                        }
                    }
                }
            }

            return result.ToArray();

        }

        // get style box by tag
        internal static String GetStyleBlock(string source, string name) 
        {

            string startTag = string.Format("/***start-{0}***/", name);
            string endTag = string.Format("/***end-{0}***/", name);

            int indexStartTag = source.IndexOf(startTag);
            int indexEndTag = source.IndexOf(endTag);

            if (indexStartTag >= 0 && indexEndTag >= 0)
                return source.Substring(indexStartTag, indexEndTag - indexStartTag).Replace(startTag, "").Replace(endTag, "");
            else
                return "";

        }

        // Create a new grid block
        internal static string NewGridRowBlock(string name) 
        {

            string lessGridRowModel = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningGridRowStyle))
            {
                lessGridRowModel = sr.ReadToEnd();
            }

            string newTag = "gridrow_" + name;
            return lessGridRowModel.Replace("-ID-", newTag);

        }

        // Get less parameters from lessPath file, both general parameters and grid parameters
        internal static string GetLessParameters(string lessPath, int pageId) 
        { 

            // Get all parameters
            string paramBlock = string.Empty;
            if (System.IO.File.Exists(lessPath))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(lessPath))
                {
                    string lessContent = sr.ReadToEnd();
                    paramBlock = GetStyleBlock(lessContent, "lessParam");
                    foreach (var gridRow in GetGridRows(pageId))
                    {
                        paramBlock = paramBlock + GetStyleBlock(lessContent, "lessParam-" + "gridrow_" + gridRow);
                    }
                }
            }
            return paramBlock;

        }

        // Get inherited pageId with tuning
        internal static int GetParentOrSelfTunedPageId(string[] path, bool preview) 
        {

            string styleTuning = preview ? @"{0}{1}.less" : "{0}{1}.css";
            foreach (var page in path.OrderByDescending(r => path.IndexOf(r)))
            {
                string stylePath = HttpContext.Current.Server.MapPath(string.Format(styleTuning, tuningStylePath, page));
                if (System.IO.File.Exists(stylePath))
                {
                    return int.Parse(page);
                }
            }

            if (preview)
                return int.Parse(path[path.Length - 1]);
            else
                return -1;

        }

        // Get stylesheet path for current page
        internal static string GetStylesheetPath(string[] path, bool preview)
        {
            string styleTuning = preview ? @"{0}{1}.less" : "{0}{1}.css";

            int tunedPageId = GetParentOrSelfTunedPageId(path, preview);

            if (tunedPageId >0)
                return string.Format(styleTuning, tuningStylePath, tunedPageId);
            else
                return string.Empty;
        }

        // Create new less file
        internal static string CreateOrUpdateLessFile(string[] path, int pageId)
        {

            string lessPath = GetStylesheetPath(path, true);

            // If less file exist, Load its  content
            string lessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(System.IO.File.Exists(HttpContext.Current.Server.MapPath(lessPath)) 
                ? HttpContext.Current.Server.MapPath(lessPath) : tuningDefaultLessPath))
            {
                lessContent = sr.ReadToEnd();
            }

            // Update with grid row style needs
            string[] gridRows = TuningUtility.GetGridRows(pageId);
            string parametersToAdd = string.Empty;
            string styleToAdd = string.Empty;
            foreach (var gridRow in gridRows)
            {
                if (string.IsNullOrEmpty(GetStyleBlock(lessContent, "gridStyle-" + "gridrow_" + gridRow))) 
                {
                    parametersToAdd += NewGridRowBlock(gridRow);
                }
            }

            // Create front directory if necesary
            if (!Directory.Exists(frontBasePath))
                Directory.CreateDirectory(frontBasePath);

            // Save less file
            if (string.IsNullOrEmpty(lessPath)) lessPath = string.Format("{0}{1}.less", tuningStylePath, pageId);
            lessContent = lessContent + Environment.NewLine + parametersToAdd;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(HttpContext.Current.Server.MapPath(lessPath)))
            {
                file.Write(lessContent);
            }

            return lessPath;

        }

        // Save and publish less style
        internal static void SaveAndPublishStyle(string parameters, string parametersGrid, int pageId) 
        {

            // Get inherited tuned pageId and path
            var contentService = ApplicationContext.Current.Services.ContentService;
            IContent content = contentService.GetById(pageId);
            int inheritedTunedPageId = TuningUtility.GetParentOrSelfTunedPageId(content.Path.Split(','), true);

            // Load inherited Less content
            string inheritedLessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(HttpContext.Current.Server.MapPath(string.Format(resultLessPath, inheritedTunedPageId))))
            {
                inheritedLessContent = sr.ReadToEnd();
            }

            // Update pageId if parameters have changed
            if (inheritedTunedPageId != pageId)
            {

                // Parse inherited less parameters
                Dictionary<string, string> dicInheritedParameters = new Dictionary<string, string>();
                foreach (Match m in Regex.Matches(GetStyleBlock(inheritedLessContent,"lessParam"), @"@([^:\s\n]*?):([^@;\n]*?);"))
                {
                    if (m.Groups.Count > 2)
                    {
                        dicInheritedParameters.Add(m.Groups[1].Value, m.Groups[2].Value.Replace("''", string.Empty));
                    }
                }

                // Read new parameters
                Dictionary<string, string> dicNewParameters = new Dictionary<string, string>();
                foreach (string parameter in parameters.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (parameter.IndexOf("@import") < 0)
                    {
                        string name = parameter.Substring(1, parameter.IndexOf(":")).Replace("@", string.Empty).Replace(":", string.Empty);
                        string value = parameter.Substring(parameter.IndexOf(":") + 1);
                        dicNewParameters.Add(name, value);
                    }
                }

                // Compare if parameters have changed
                bool noChange = true;
                foreach (string keys in dicNewParameters.Keys)
                {
                    if (!dicInheritedParameters.Keys.Where(r => r == keys).Any() || dicNewParameters[keys] != dicInheritedParameters[keys])
                    {
                        noChange = false;
                        break;
                    }
                }

                if (noChange)
                    pageId = inheritedTunedPageId;



                // Compare if parameters have changed
                if (dicInheritedParameters.OrderBy(r => r.Key).SequenceEqual(dicNewParameters.OrderBy(r => r.Key)))
                    pageId = inheritedTunedPageId;

            }

            // Create front directory if necesary
            if (!Directory.Exists(frontBasePath))
                Directory.CreateDirectory(frontBasePath);

            // Prepare parameters and gf block and replace with the new value
            string newParamBlock = string.Empty;
            string newGfBlock = string.Empty;
            foreach (string parameter in parameters.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameter.IndexOf("@import") < 0)
                {
                    newParamBlock += (parameter + ";").Replace(":;", ":'';") + Environment.NewLine;
                }
                else
                {
                    newGfBlock += parameter + ";" + Environment.NewLine;
                }
            }
            inheritedLessContent = inheritedLessContent.Replace(GetStyleBlock(inheritedLessContent, "lessParam"), Environment.NewLine + newParamBlock);
            inheritedLessContent = inheritedLessContent.Replace(GetStyleBlock(inheritedLessContent, "gf"), Environment.NewLine + newGfBlock);

            // Prepare grid parameters
            string newGridParamBlock = string.Empty;
            foreach (string parameter in parametersGrid.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string name = parameter.Substring(0, parameter.IndexOf(":"));
                string value = parameter.Substring(parameter.IndexOf(":") + 1);
                if (string.IsNullOrEmpty(value)) value = "''";
                inheritedLessContent = Regex.Replace(inheritedLessContent, string.Format("{0}:([^;\n]*);", name), string.Format("{0}:{1};", name, value));

            }

            // Save less file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId))))
            {
                file.Write(inheritedLessContent);
            }

            // Compile the Less file
            string compiledStyle = GetCssFromLessString(inheritedLessContent, false, true, true).Replace("@import", "@IMPORT");

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId))))
            {
                file.Write(compiledStyle);
            }

        }

        // Delete tuning style
        internal static void DeleteStyle(int pageId)
        {

            // Path to the less and css files
            string newResultLessPath = HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId));
            string newResultCssPath = HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId));

            // Delete all style file for this page
            System.IO.File.Delete(newResultLessPath);
            System.IO.File.Delete(newResultCssPath);

        }

        // Compile and compress less style
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
