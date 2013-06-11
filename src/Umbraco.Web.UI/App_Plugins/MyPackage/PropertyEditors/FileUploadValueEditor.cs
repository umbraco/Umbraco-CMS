
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using umbraco.cms.businesslogic.Files;
using Umbraco.Core;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    /// <summary>
    /// The editor for the file upload property editor
    /// </summary>
    internal class FileUploadValueEditor : ValueEditor
    {
        //TODO: This needs to come from pre-values
        private const string ThumbnailSizes = "100";

        /// <summary>
        /// Overrides the deserialize value so that we can save the file accordingly
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue">
        /// The current value persisted for this property. This will allow us to determine if we want to create a new
        /// file path or use the existing file path.
        /// </param>
        /// <returns></returns>
        public override object DeserializeValue(ContentPropertyData editorValue, object currentValue)
        {

            //if the value is the same then just return the current value
            if (currentValue != null && editorValue.Value == currentValue.ToString())
            {
                return currentValue;
            }

            //check the editorValue value to see if we need to clear the files or not
            var clear = false;
            if (editorValue.Value.IsNullOrWhiteSpace() == false)
            {
                var parsed = JObject.Parse(editorValue.Value);
                clear = parsed["clearFiles"].Value<bool>();
            }

            //parse the current value
            var currentPersistedValues = new JArray();
            if (currentValue != null)
            {
                try
                {
                    currentPersistedValues = JArray.Parse(currentValue.ToString());
                }
                catch (JsonReaderException)
                {
                    //if we cannot parse, we'll ignore the error and start again, there must be a bad value stored for some reason                            
                }
            }

            var newValue = new JArray();

            if (clear)
            {
                //TODO: Need to delete our old files!

                return newValue.ToString(Formatting.None);
            }
            
            //check for any files
            if (editorValue.AdditionalData.ContainsKey("files"))
            {
                var files = editorValue.AdditionalData["files"] as IEnumerable<ContentItemFile>;
                if (files != null)
                {
                    var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

                    //now we just need to move the files to where they should be
                    var filesAsArray = files.ToArray();
                    for (var i = 0; i < filesAsArray.Length; i++)
                    {
                        var file = filesAsArray[i];

                        var currentPersistedFile = currentPersistedValues.Count >= (i + 1)
                                                       ? currentPersistedValues[i]["file"].ToString()
                                                       : "";

                        var name = IOHelper.SafeFileName(file.FileName.Substring(file.FileName.LastIndexOf(IOHelper.DirSepChar) + 1, file.FileName.Length - file.FileName.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower());

                        var subfolder = UmbracoSettings.UploadAllowDirectories
                                            ? currentPersistedFile.Replace(fs.GetUrl("/"), "").Split('/')[0]
                                            : currentPersistedFile.Substring(currentPersistedFile.LastIndexOf("/", StringComparison.Ordinal) + 1).Split('-')[0];

                        int subfolderId;
                        var numberedFolder = int.TryParse(subfolder, out subfolderId)
                                                 ? subfolderId.ToString(CultureInfo.InvariantCulture)
                                                 : MediaSubfolderCounter.Current.Increment().ToString(CultureInfo.InvariantCulture);

                        var fileName = UmbracoSettings.UploadAllowDirectories
                                           ? Path.Combine(numberedFolder, name)
                                           : numberedFolder + "-" + name;

                        using (var fileStream = File.OpenRead(file.TempFilePath))
                        {
                            var umbracoFile = UmbracoFile.Save(fileStream, fileName);

                            //create json to be saved
                            var forPersisting = JObject.FromObject(new
                                {
                                    file = umbracoFile.Url,
                                    isImage = false
                                });


                            if (umbracoFile.SupportsResizing)
                            {
                                forPersisting["isImage"] = true;

                                // make default thumbnail
                                umbracoFile.Resize(100, "thumb");

                                // additional thumbnails configured as prevalues on the DataType
                                char sep = (ThumbnailSizes.Contains("") == false && ThumbnailSizes.Contains(",")) ? ',' : ';';

                                foreach (string thumb in ThumbnailSizes.Split(sep))
                                {
                                    int thumbSize;
                                    if (thumb != "" && int.TryParse(thumb, out thumbSize))
                                    {
                                        umbracoFile.Resize(thumbSize, string.Format("thumb_{0}", thumbSize));
                                    }
                                }
                            }
                            
                            //add this to the persisted values
                            newValue.Add(forPersisting);
                        }
                        //now remove the temp file
                        File.Delete(file.TempFilePath);
                    }

                    //TODO: We need to remove any files that were previously persisted but are no longer persisted. FOr example, if we
                    // uploaded 5 files before and then only uploaded 3, then the last two should be deleted.

                    return newValue.ToString(Formatting.None);
                }
            }

            //if we've made it here, we had no files to save and we were not clearing anything so just persist the same value we had before
            return currentValue;
        }
    }
}