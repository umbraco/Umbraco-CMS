using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Parses the Main.js file and replaces all tokens accordingly.
    /// </summary>
    internal class ManifestParser
    {
        private readonly DirectoryInfo _pluginsDir;
        
        //used to strip comments
        private static readonly Regex CommentsSurround = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/", RegexOptions.Compiled);
        private static readonly Regex CommentsLine = new Regex(@"//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);

        public ManifestParser(DirectoryInfo pluginsDir)
        {
            if (pluginsDir == null) throw new ArgumentNullException("pluginsDir");
            _pluginsDir = pluginsDir;
        }

        /// <summary>
        /// Parse the property editors from the json array
        /// </summary>
        /// <param name="jsonEditors"></param>
        /// <returns></returns>
        internal static IEnumerable<PropertyEditor> GetPropertyEditors(JArray jsonEditors)
        {
            return JsonConvert.DeserializeObject<IEnumerable<PropertyEditor>>(
                jsonEditors.ToString(), 
                new PropertyEditorConverter(),
                new PreValueFieldConverter());
        }

        /// <summary>
        /// Parse the property editors from the json array
        /// </summary>
        /// <param name="jsonEditors"></param>
        /// <returns></returns>
        internal static IEnumerable<ParameterEditor> GetParameterEditors(JArray jsonEditors)
        {
            return JsonConvert.DeserializeObject<IEnumerable<ParameterEditor>>(
                jsonEditors.ToString(),
                new ParameterEditorConverter());
        }
        
        /// <summary>
        /// Get all registered manifests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageManifest> GetManifests()
        {
            //get all Manifest.js files in the appropriate folders
            var manifestFileContents = GetAllManifestFileContents(_pluginsDir);
            return CreateManifests(manifestFileContents.ToArray());
        }

        /// <summary>
        /// Get the file contents from all declared manifest files
        /// </summary>
        /// <param name="currDir"></param>
        /// <returns></returns>
        private IEnumerable<string> GetAllManifestFileContents(DirectoryInfo currDir)
        {
            var depth = FolderDepth(_pluginsDir, currDir);
            
            if (depth < 1)
            {
                var dirs = currDir.GetDirectories();
                var result = new List<string>();
                foreach (var d in dirs)
                {
                    result.AddRange(GetAllManifestFileContents(d));
                }
                return result;
            }

            //look for files here
            return currDir.GetFiles("Package.manifest")
                          .Select(f => File.ReadAllText(f.FullName))
                          .ToList();
        }

        /// <summary>
        /// Get the folder depth compared to the base folder
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="currDir"></param>
        /// <returns></returns>
        internal static int FolderDepth(DirectoryInfo baseDir, DirectoryInfo currDir)
        {
            var removed = currDir.FullName.Remove(0, baseDir.FullName.Length).TrimStart('\\').TrimEnd('\\');
            return removed.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Creates a list of PropertyEditorManifest from the file contents of each manifest file
        /// </summary>
        /// <param name="manifestFileContents"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that comments are removed (but they have to be /* */ style comments
        /// and ensures that virtual paths are replaced with real ones
        /// </remarks>
        internal static IEnumerable<PackageManifest> CreateManifests(params string[] manifestFileContents)
        {
            var result = new List<PackageManifest>();
            foreach (var m in manifestFileContents)
            {
                if (m.IsNullOrWhiteSpace()) continue;

                //remove any comments first
                var replaced = CommentsSurround.Replace(m, match => " ");
                replaced = CommentsLine.Replace(replaced, match => "");

                JObject deserialized;
                try
                {
                    deserialized = JsonConvert.DeserializeObject<JObject>(replaced);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ManifestParser>("An error occurred parsing manifest with contents: " + m, ex);
                    continue;
                }

                //validate the javascript
                var init = deserialized.Properties().Where(x => x.Name == "javascript").ToArray();
                if (init.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'javascript' element");
                }

                //validate the css
                var cssinit = deserialized.Properties().Where(x => x.Name == "css").ToArray();
                if (cssinit.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'css' element");
                }

                //validate the property editors section
                var propEditors = deserialized.Properties().Where(x => x.Name == "propertyEditors").ToArray();
                if (propEditors.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'propertyEditors' element");
                }

                var jConfig = init.Any() ? (JArray)deserialized["javascript"] : new JArray();
                ReplaceVirtualPaths(jConfig);

                var cssConfig = cssinit.Any() ? (JArray)deserialized["css"] : new JArray();
                ReplaceVirtualPaths(cssConfig);

                //replace virtual paths for each property editor
                if (deserialized["propertyEditors"] != null)
                {
                    foreach (JObject p in deserialized["propertyEditors"])
                    {
                        if (p["editor"] != null)
                        {
                            ReplaceVirtualPaths((JObject) p["editor"]);
                        }
                        if (p["preValues"] != null)
                        {
                            ReplaceVirtualPaths((JObject)p["preValues"]);
                        }
                    }
                }
                
                var manifest = new PackageManifest()
                    {
                        JavaScriptInitialize = jConfig,
                        StylesheetInitialize = cssConfig,
                        PropertyEditors = propEditors.Any() ? (JArray)deserialized["propertyEditors"] : new JArray(),
                        ParameterEditors = propEditors.Any() ? (JArray)deserialized["parameterEditors"] : new JArray()
                    };
                result.Add(manifest);
            }
            return result;
        }

        /// <summary>
        /// Replaces any virtual paths found in properties
        /// </summary>
        /// <param name="jarr"></param>
        private static void ReplaceVirtualPaths(JArray jarr)
        {
            foreach (var i in jarr)
            {
                ReplaceVirtualPaths(i);
            }
        }

        /// <summary>
        /// Replaces any virtual paths found in properties
        /// </summary>
        /// <param name="jToken"></param>
        private static void ReplaceVirtualPaths(JToken jToken)
        {
            if (jToken.Type == JTokenType.Object)
            {
                //recurse
                ReplaceVirtualPaths((JObject)jToken);
            }
            else
            {
                var value = jToken as JValue;
                if (value != null)
                {
                    if (value.Type == JTokenType.String)
                    {
                        if (value.Value<string>().StartsWith("~/"))
                        {
                            //replace the virtual path
                            value.Value = IOHelper.ResolveUrl(value.Value<string>());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces any virtual paths found in properties
        /// </summary>
        /// <param name="jObj"></param>
        private static void ReplaceVirtualPaths(JObject jObj)
        {
            foreach (var p in jObj.Properties().Select(x => x.Value))
            {
                ReplaceVirtualPaths(p);
            }
        }

        /// <summary>
        /// Merges two json objects together
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="donor"></param>
        /// <param name="keepOriginal">set to true if we will keep the receiver value if the proeprty already exists</param>
        /// <remarks>
        /// taken from 
        /// http://stackoverflow.com/questions/4002508/does-c-sharp-have-a-library-for-parsing-multi-level-cascading-json/4002550#4002550
        /// </remarks>
        internal static void MergeJObjects(JObject receiver, JObject donor, bool keepOriginal = false)
        {
            foreach (var property in donor)
            {
                var receiverValue = receiver[property.Key] as JObject;
                var donorValue = property.Value as JObject;
                if (receiverValue != null && donorValue != null)
                {
                    MergeJObjects(receiverValue, donorValue);
                }
                else if (receiver[property.Key] == null || !keepOriginal)
                {
                    receiver[property.Key] = property.Value;
                }
            }
        }

        /// <summary>
        /// Merges the donor array values into the receiver array
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="donor"></param>
        internal static void MergeJArrays(JArray receiver, JArray donor)
        {
            foreach (var item in donor)
            {
                if (!receiver.Any(x => x.Equals(item)))
                {
                    receiver.Add(item);   
                }
            }
        }

        
    }
}