using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// The value editor for the file upload property editor.
    /// </summary>
    internal class FileUploadPropertyValueEditor : PropertyValueEditorWrapper
    {
        private readonly MediaFileSystem _mediaFileSystem;

        public FileUploadPropertyValueEditor(PropertyValueEditor wrapped, MediaFileSystem mediaFileSystem)
            : base(wrapped)
        {
            _mediaFileSystem = mediaFileSystem;
        }

        /// <summary>
        /// Converts the value received from the editor into the value can be stored in the database.
        /// </summary>
        /// <param name="editorValue">The value received from the editor.</param>
        /// <param name="currentValue">The current value of the property</param>
        /// <returns>The converted value.</returns>
        /// <remarks>
        /// <para>The <paramref name="currentValue"/> is used to re-use the folder, if possible.</para>
        /// <para>The <paramref name="editorValue"/> is value passed in from the editor. We normally don't care what
        /// the editorValue.Value is set to because we are more interested in the files collection associated with it,
        /// however we do care about the value if we are clearing files. By default the editorValue.Value will just
        /// be set to the name of the file - but again, we just ignore this and deal with the file collection in
        /// editorValue.AdditionalData.ContainsKey("files")</para>
        /// <para>We only process ONE file. We understand that the current value may contain more than one file,
        /// and that more than one file may be uploaded, so we take care of them all, but we only store ONE file.
        /// Other places (FileUploadPropertyEditor...) do NOT deal with multiple files, and our logic for reusing
        /// folders would NOT work, etc.</para>
        /// </remarks>
        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            currentValue = currentValue ?? string.Empty;

            // at that point,
            // currentValue is either empty or "/media/path/to/img.jpg"
            // editorValue.Value is { "clearFiles": true } or { "selectedFiles": "img1.jpg,img2.jpg" }
            // comparing them makes little sense

            // check the editorValue value to see whether we need to clear files
            var editorJsonValue = editorValue.Value as JObject;
            var clears = editorJsonValue != null && editorJsonValue["clearFiles"] != null && editorJsonValue["clearFiles"].Value<bool>();
            var uploads = editorValue.AdditionalData.ContainsKey("files") && editorValue.AdditionalData["files"] is IEnumerable<ContentItemFile>;

            // nothing = no changes, return what we have already (leave existing files intact)
            if (clears == false && uploads == false)
                return currentValue;

            // get the current file paths
            var currentPaths = currentValue.ToString()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => _mediaFileSystem.GetRelativePath(x)) // get the fs-relative path
                .ToArray();

            // if clearing, remove these files and return
            if (clears)
            {
                foreach (var pathToRemove in currentPaths)
                    _mediaFileSystem.DeleteFile(pathToRemove, true);
                return string.Empty; // no more files
            }

            // ensure we have the required guids
            if (editorValue.AdditionalData.ContainsKey("cuid") == false // for the content item
                || editorValue.AdditionalData.ContainsKey("puid") == false) // and the property type
                throw new Exception("Missing cuid/puid additional data.");
            var cuido = editorValue.AdditionalData["cuid"];
            var puido = editorValue.AdditionalData["puid"];
            if ((cuido is Guid) == false || (puido is Guid) == false)
                throw new Exception("Invalid cuid/puid additional data.");
            var cuid = (Guid) cuido;
            var puid = (Guid) puido;
            if (cuid == Guid.Empty || puid == Guid.Empty)
                throw new Exception("Invalid cuid/puid additional data.");

            // process the files
            var files = ((IEnumerable<ContentItemFile>) editorValue.AdditionalData["files"]).ToArray();

            var newPaths = new List<string>();
            const int maxLength = 1; // we only process ONE file
            for (var i = 0; i < maxLength /*files.Length*/; i++)
            {
                var file = files[i];

                // skip invalid files
                if (UploadFileTypeValidator.ValidateFileExtension(file.FileName) == false)
                    continue;

                // get the filepath
                // in case we are using the old path scheme, try to re-use numbers (bah...)
                var reuse = i < currentPaths.Length ? currentPaths[i] : null; // this would be WRONG with many files
                var filepath = _mediaFileSystem.GetMediaPath(file.FileName, reuse, cuid, puid); // fs-relative path

                using (var filestream = File.OpenRead(file.TempFilePath))
                {
                    _mediaFileSystem.AddFile(filepath, filestream, true); // must overwrite!

                    var ext = _mediaFileSystem.GetExtension(filepath);
                    if (_mediaFileSystem.IsImageFile(ext) && ext != ".svg")
                    {
                        var preValues = editorValue.PreValues.FormatAsDictionary();
                        var sizes = preValues.Any() ? preValues.First().Value.Value : string.Empty;
                        using (var image = Image.FromStream(filestream))
                            _mediaFileSystem.GenerateThumbnails(image, filepath, sizes);
                    }

                    // all related properties (auto-fill) are managed by FileUploadPropertyEditor
                    // when the content is saved (through event handlers)

                    newPaths.Add(filepath);
                }
            }

            // remove all temp files
            foreach (var file in files)
                File.Delete(file.TempFilePath);

            // remove files that are not there anymore
            foreach (var pathToRemove in currentPaths.Except(newPaths))
                _mediaFileSystem.DeleteFile(pathToRemove, true);


            return string.Join(",", newPaths.Select(x => _mediaFileSystem.GetUrl(x)));
        }
    }
}
