using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using File = System.IO.File;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// The value editor for the image cropper property editor.
    /// </summary>
    internal class ImageCropperPropertyValueEditor : PropertyValueEditorWrapper
    {
        private readonly MediaFileSystem _mediaFileSystem;

        public ImageCropperPropertyValueEditor(PropertyValueEditor wrapped, MediaFileSystem mediaFileSystem)
            : base(wrapped)
        {
            _mediaFileSystem = mediaFileSystem;
        }

        /// <summary>
        /// This is called to merge in the prevalue crops with the value that is saved - similar to the property value converter for the front-end
        /// </summary>

        public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var val = base.ConvertDbToEditor(property, propertyType, dataTypeService);

            var json = val as JObject;
            if (json != null)
            {
                ImageCropperValueConverter.MergePreValues(json, dataTypeService, propertyType.DataTypeDefinitionId);
                return json;
            }

            return val;
        }
        /// <summary>
        /// Converts the value received from the editor into the value can be stored in the database.
        /// </summary>
        /// <param name="editorValue">The value received from the editor.</param>
        /// <param name="currentValue">The current value of the property</param>
        /// <returns>The converted value.</returns>
        /// <remarks>
        /// <para>The <paramref name="currentValue"/> is used to re-use the folder, if possible.</para>
        /// <para>editorValue.Value is used to figure out editorFile and, if it has been cleared, remove the old file - but
        /// it is editorValue.AdditionalData["files"] that is used to determine the actual file that has been uploaded.</para>
        /// </remarks>
        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            // get the current path
            var currentPath = string.Empty;
            try
            {
                var svalue = currentValue as string;
                var currentJson = string.IsNullOrWhiteSpace(svalue) ? null : JObject.Parse(svalue);
                if (currentJson != null && currentJson["src"] != null)
                    currentPath = currentJson["src"].Value<string>();
            }
            catch (Exception ex)
            {
                // for some reason the value is invalid so continue as if there was no value there
                LogHelper.WarnWithException<ImageCropperPropertyValueEditor>("Could not parse current db value to a JObject.", ex);
            }
            if (string.IsNullOrWhiteSpace(currentPath) == false)
                currentPath = _mediaFileSystem.GetRelativePath(currentPath);

            // get the new json and path
            JObject editorJson = null;
            var editorFile = string.Empty;
            if (editorValue.Value != null)
            {
                editorJson = editorValue.Value as JObject;
                if (editorJson != null && editorJson["src"] != null)
                    editorFile = editorJson["src"].Value<string>();
            }

            // ensure we have the required guids
            if (editorValue.AdditionalData.ContainsKey("cuid") == false // for the content item
                || editorValue.AdditionalData.ContainsKey("puid") == false) // and the property type
                throw new Exception("Missing cuid/puid additional data.");
            var cuido = editorValue.AdditionalData["cuid"];
            var puido = editorValue.AdditionalData["puid"];
            if ((cuido is Guid) == false || (puido is Guid) == false)
                throw new Exception("Invalid cuid/puid additional data.");
            var cuid = (Guid)cuido;
            var puid = (Guid)puido;
            if (cuid == Guid.Empty || puid == Guid.Empty)
                throw new Exception("Invalid cuid/puid additional data.");

            // editorFile is empty whenever a new file is being uploaded
            // or when the file is cleared (in which case editorJson is null)
            // else editorFile contains the unchanged value

            var uploads = editorValue.AdditionalData.ContainsKey("files") && editorValue.AdditionalData["files"] is IEnumerable<ContentItemFile>;
            var files = uploads ? ((IEnumerable<ContentItemFile>)editorValue.AdditionalData["files"]).ToArray() : new ContentItemFile[0];
            var file = uploads ? files.FirstOrDefault() : null;

            if (file == null) // not uploading a file
            {
                // if editorFile is empty then either there was nothing to begin with,
                // or it has been cleared and we need to remove the file - else the
                // value is unchanged.
                if (string.IsNullOrWhiteSpace(editorFile) && string.IsNullOrWhiteSpace(currentPath) == false)
                {
                    _mediaFileSystem.DeleteFile(currentPath, true);
                    return null; // clear
                }

                return editorJson == null ? null : editorJson.ToString(); // unchanged
            }

            // process the file
            var filepath = editorJson == null ? null : ProcessFile(editorValue, file, currentPath, cuid, puid);

            // remove all temp files
            foreach (var f in files)
                File.Delete(f.TempFilePath);

            // remove current file if replaced
            if (currentPath != filepath && string.IsNullOrWhiteSpace(currentPath) == false)
                _mediaFileSystem.DeleteFile(currentPath, true);

            // update json and return
            if (editorJson == null) return null;
            editorJson["src"] = filepath == null ? string.Empty : _mediaFileSystem.GetUrl(filepath);
            return editorJson.ToString();
        }

        private string ProcessFile(ContentPropertyData editorValue, ContentItemFile file, string currentPath, Guid cuid, Guid puid)
        {
            // process the file
            // no file, invalid file, reject change
            if (UploadFileTypeValidator.ValidateFileExtension(file.FileName) == false)
                return null;

            // get the filepath
            // in case we are using the old path scheme, try to re-use numbers (bah...)
            var filepath = _mediaFileSystem.GetMediaPath(file.FileName, currentPath, cuid, puid); // fs-relative path

            using (var filestream = File.OpenRead(file.TempFilePath))
            {
                _mediaFileSystem.AddFile(filepath, filestream, true); // must overwrite!

                var ext = _mediaFileSystem.GetExtension(filepath);
                if (_mediaFileSystem.IsImageFile(ext) && ext != ".svg")
                {
                    var preValues = editorValue.PreValues.FormatAsDictionary();
                    var sizes = preValues.Any() ? preValues.First().Value.Value : string.Empty;
                    try
                    {
                        using (var image = Image.FromStream(filestream))
                            _mediaFileSystem.GenerateThumbnails(image, filepath, sizes);
                    }
                    catch (ArgumentException ex)
                    {
                        // send any argument errors caused by the thumbnail generation to the log instead of failing miserably 
                        LogHelper.WarnWithException<ImageCropperPropertyValueEditor>("Could not extract image thumbnails.", ex);
                    }
                }

                // all related properties (auto-fill) are managed by ImageCropperPropertyEditor
                // when the content is saved (through event handlers)
            }

            return filepath;
        }

        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null || string.IsNullOrEmpty(property.Value.ToString()))
               return null;

            // if we dont have a json structure, we will get it from the property type
            var val = property.Value.ToString();
            if (val.DetectIsJson())
                return val;

            // more magic here ;-(
            var config = dataTypeService.GetPreValuesByDataTypeId(propertyType.DataTypeDefinitionId).FirstOrDefault();
            var crops = string.IsNullOrEmpty(config) ? "[]" : config;
            var newVal = "{src: '" + val + "', crops: " + crops + "}";
            return newVal;
        }
    }
}
