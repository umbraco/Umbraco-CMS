using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// The value editor for the image cropper property editor.
    /// </summary>
    internal class ImageCropperPropertyValueEditor : DataValueEditor // fixme core vs web?
    {
        private readonly ILogger _logger;
        private readonly MediaFileSystem _mediaFileSystem;

        public ImageCropperPropertyValueEditor(DataEditorAttribute attribute, ILogger logger, MediaFileSystem mediaFileSystem)
            : base(attribute)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
        }

        /// <summary>
        /// This is called to merge in the prevalue crops with the value that is saved - similar to the property value converter for the front-end
        /// </summary>

        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
        {
            var val = property.GetValue(culture, segment);
            if (val == null) return null;

            ImageCropperValue value;
            try
            {
                value = JsonConvert.DeserializeObject<ImageCropperValue>(val.ToString());
            }
            catch
            {
                value = new ImageCropperValue { Src = val.ToString() };
            }

            var dataType = dataTypeService.GetDataType(property.PropertyType.DataTypeId);
            if (dataType?.Configuration != null)
                value.ApplyConfiguration(dataType.ConfigurationAs<ImageCropperConfiguration>());

            return value;
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
        public override object FromEditor(ContentPropertyData editorValue, object currentValue)
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
                _logger.Warn<ImageCropperPropertyValueEditor>(ex, "Could not parse current db value to a JObject.");
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
            var cuid = editorValue.ContentKey;
            if (cuid == Guid.Empty) throw new Exception("Invalid content key.");
            var puid = editorValue.PropertyTypeKey;
            if (puid == Guid.Empty) throw new Exception("Invalid property type key.");

            // editorFile is empty whenever a new file is being uploaded
            // or when the file is cleared (in which case editorJson is null)
            // else editorFile contains the unchanged value

            var uploads = editorValue.Files;
            if (uploads == null) throw new Exception("Invalid files.");
            var file = uploads.Length > 0 ? uploads[0] : null;

            if (file == null) // not uploading a file
            {
                // if editorFile is empty then either there was nothing to begin with,
                // or it has been cleared and we need to remove the file - else the
                // value is unchanged.
                if (string.IsNullOrWhiteSpace(editorFile) && string.IsNullOrWhiteSpace(currentPath) == false)
                {
                    _mediaFileSystem.DeleteFile(currentPath);
                    return null; // clear
                }

                return editorJson?.ToString(); // unchanged
            }

            // process the file
            var filepath = editorJson == null ? null : ProcessFile(editorValue, file, currentPath, cuid, puid);

            // remove all temp files
            foreach (var f in uploads)
                File.Delete(f.TempFilePath);

            // remove current file if replaced
            if (currentPath != filepath && string.IsNullOrWhiteSpace(currentPath) == false)
                _mediaFileSystem.DeleteFile(currentPath);

            // update json and return
            if (editorJson == null) return null;
            editorJson["src"] = filepath == null ? string.Empty : _mediaFileSystem.GetUrl(filepath);
            return editorJson.ToString();
        }

        private string ProcessFile(ContentPropertyData editorValue, ContentPropertyFile file, string currentPath, Guid cuid, Guid puid)
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
                //TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
                // since we'd need to be able to return values for other properties from these methods

                _mediaFileSystem.AddFile(filepath, filestream, true); // must overwrite!
            }

            return filepath;
        }


        public override string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return null;

            // if we dont have a json structure, we will get it from the property type
            var val = value.ToString();
            if (val.DetectIsJson())
                return val;

            // more magic here ;-(
            var configuration = dataTypeService.GetDataType(propertyType.DataTypeId).ConfigurationAs<ImageCropperConfiguration>();
            var crops = configuration?.Crops ?? Array.Empty<ImageCropperConfiguration.Crop>();
            return "{src: '" + val + "', crops: " + crops + "}";
        }
    }
}
