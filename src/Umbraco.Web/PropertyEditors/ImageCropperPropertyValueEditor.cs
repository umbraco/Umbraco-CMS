using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.PropertyEditors
{
    internal class ImageCropperPropertyValueEditor : PropertyValueEditorWrapper
    {
        public ImageCropperPropertyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
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


            string oldFile = string.Empty;
            string newFile = string.Empty;
            JObject newJson = null;
            JObject oldJson = null;

            //get the old src path
            if (currentValue != null && string.IsNullOrEmpty(currentValue.ToString()) == false)
            {
                try
                {
                    oldJson = JObject.Parse(currentValue.ToString());
                }
                catch (Exception ex)
                {
                    //for some reason the value is invalid so continue as if there was no value there
                    LogHelper.WarnWithException<ImageCropperPropertyValueEditor>("Could not parse current db value to a JObject", ex);
                }

                if (oldJson != null && oldJson["src"] != null)
                {
                    oldFile = oldJson["src"].Value<string>();
                }
            }

            //get the new src path
            if (editorValue.Value != null)
            {
                newJson = editorValue.Value as JObject;
                if (newJson != null && newJson["src"] != null)
                {
                    newFile = newJson["src"].Value<string>();
                }
            }

            //compare old and new src path
            //if not alike, that means we have a new file, or delete the current one... 
            if (string.IsNullOrEmpty(newFile) || editorValue.AdditionalData.ContainsKey("files"))
            {
                var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

                //if we have an existing file, delete it
                if (string.IsNullOrEmpty(oldFile) == false)
                    fs.DeleteFile(fs.GetRelativePath(oldFile), true);
                else
                    oldFile = string.Empty;

                //if we have a new file, add it to the media folder and set .src

                if (editorValue.AdditionalData.ContainsKey("files"))
                {
                    var files = editorValue.AdditionalData["files"] as IEnumerable<ContentItemFile>;
                    if (files != null && files.Any())
                    {
                        var file = files.First();

                        if (UploadFileTypeValidator.ValidateFileExtension(file.FileName))
                        {
                            //create name and folder number
                            var name = IOHelper.SafeFileName(file.FileName.Substring(file.FileName.LastIndexOf(IOHelper.DirSepChar) + 1, file.FileName.Length - file.FileName.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower());

                            //try to reuse the folder number from the current file
                            var subfolder = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                                                ? oldFile.Replace(fs.GetUrl("/"), "").Split('/')[0]
                                                : oldFile.Substring(oldFile.LastIndexOf("/", StringComparison.Ordinal) + 1).Split('-')[0];

                            //if we dont find one, create a new one
                            int subfolderId;
                            var numberedFolder = int.TryParse(subfolder, out subfolderId)
                                                     ? subfolderId.ToString(CultureInfo.InvariantCulture)
                                                     : MediaSubfolderCounter.Current.Increment().ToString(CultureInfo.InvariantCulture);

                            //set a file name or full path
                            var fileName = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                                               ? Path.Combine(numberedFolder, name)
                                               : numberedFolder + "-" + name;

                            //save file and assign to the json
                            using (var fileStream = System.IO.File.OpenRead(file.TempFilePath))
                            {
                                var umbracoFile = UmbracoMediaFile.Save(fileStream, fileName);
                                newJson["src"] = umbracoFile.Url;

                                return newJson.ToString();
                            }
                        }
                    }
                }
            }

            //incase we submit nothing back
            if (editorValue.Value == null)
                return null;

            return editorValue.Value.ToString();
        }
        
        

        public override string ConvertDbToString(Property property, PropertyType propertyType, Core.Services.IDataTypeService dataTypeService)
        {
            if(property.Value == null || string.IsNullOrEmpty(property.Value.ToString()))
               return null;

            //if we dont have a json structure, we will get it from the property type
            var val = property.Value.ToString();
            if (val.DetectIsJson())
                return val;

            var config = dataTypeService.GetPreValuesByDataTypeId(propertyType.DataTypeDefinitionId).FirstOrDefault();
            var crops = !string.IsNullOrEmpty(config) ? config : "[]";
            var newVal = "{src: '" + val + "', crops: " + crops + "}"; 
            return newVal;
        }
    }

        
}
