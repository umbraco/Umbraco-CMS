using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using umbraco.cms.businesslogic.Files;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// The editor for the file upload property editor
    /// </summary>
    internal class FileUploadPropertyValueEditor : PropertyValueEditorWrapper
    {
        public FileUploadPropertyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// Overrides the deserialize value so that we can save the file accordingly
        /// </summary>
        /// <param name="editorValue">
        /// This is value passed in from the editor. We normally don't care what the editorValue.Value is set to because
        /// we are more interested in the files collection associated with it, however we do care about the value if we 
        /// are clearing files. By default the editorValue.Value will just be set to the name of the file (but again, we
        /// just ignore this and deal with the file collection in editorValue.AdditionalData.ContainsKey("files") )
        /// </param>
        /// <param name="currentValue">
        /// The current value persisted for this property. This will allow us to determine if we want to create a new
        /// file path or use the existing file path.
        /// </param>
        /// <returns></returns>
        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            if (currentValue == null)
            {
                currentValue = string.Empty;
            }

            //if the value is the same then just return the current value so we don't re-process everything
            if (string.IsNullOrEmpty(currentValue.ToString()) == false && editorValue.Value == currentValue.ToString())
            {
                return currentValue;
            }

            //check the editorValue value to see if we need to clear the files or not.
            var clear = false;
            var json = editorValue.Value as JObject;
            if (json != null && json["clearFiles"] != null && json["clearFiles"].Value<bool>())
            {
                clear = json["clearFiles"].Value<bool>();
            }

            var currentPersistedValues = new string[] {};
            if (string.IsNullOrEmpty(currentValue.ToString()) == false)
            {
                currentPersistedValues = currentValue.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            }

            var newValue = new List<string>();

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            if (clear)
            {
                //Remove any files that are saved for this item
                foreach (var toRemove in currentPersistedValues)
                {
                    fs.DeleteFile(fs.GetRelativePath(toRemove), true);
                }
                return "";
            }
            
            //check for any files
            if (editorValue.AdditionalData.ContainsKey("files"))
            {
                var files = editorValue.AdditionalData["files"] as IEnumerable<ContentItemFile>;
                if (files != null)
                {
                    //now we just need to move the files to where they should be
                    var filesAsArray = files.ToArray();
                    //a list of all of the newly saved files so we can compare with the current saved files and remove the old ones
                    var savedFilePaths = new List<string>();
                    for (var i = 0; i < filesAsArray.Length; i++)
                    {
                        var file = filesAsArray[i];

                        //don't continue if this is not allowed!
                        if (UploadFileTypeValidator.ValidateFileExtension(file.FileName) == false)
                        {
                            continue;
                        }

                        //TODO: ALl of this naming logic needs to be put into the ImageHelper and then we need to change ContentExtensions to do the same!

                        var currentPersistedFile = currentPersistedValues.Length >= (i + 1)
                                                       ? currentPersistedValues[i]
                                                       : "";

                        var name = IOHelper.SafeFileName(file.FileName.Substring(file.FileName.LastIndexOf(IOHelper.DirSepChar) + 1, file.FileName.Length - file.FileName.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower());

                        var subfolder = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                                            ? currentPersistedFile.Replace(fs.GetUrl("/"), "").Split('/')[0]
                                            : currentPersistedFile.Substring(currentPersistedFile.LastIndexOf("/", StringComparison.Ordinal) + 1).Split('-')[0];

                        int subfolderId;
                        var numberedFolder = int.TryParse(subfolder, out subfolderId)
                                                 ? subfolderId.ToString(CultureInfo.InvariantCulture)
                                                 : MediaSubfolderCounter.Current.Increment().ToString(CultureInfo.InvariantCulture);

                        var fileName = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                                           ? Path.Combine(numberedFolder, name)
                                           : numberedFolder + "-" + name;

                        using (var fileStream = File.OpenRead(file.TempFilePath))
                        {
                            var umbracoFile = UmbracoMediaFile.Save(fileStream, fileName);

                            newValue.Add(umbracoFile.Url);
                            //add to the saved paths
                            savedFilePaths.Add(umbracoFile.Url);
                        }
                        //now remove the temp file
                        File.Delete(file.TempFilePath);   
                    }
                    
                    //Remove any files that are no longer saved for this item
                    foreach (var toRemove in currentPersistedValues.Except(savedFilePaths))
                    {
                        fs.DeleteFile(fs.GetRelativePath(toRemove), true);
                    }
                    

                    return string.Join(",", newValue);
                }
            }

            //if we've made it here, we had no files to save and we were not clearing anything so just persist the same value we had before
            return currentValue;
        }

    }
}