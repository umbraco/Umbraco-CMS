﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// The value editor for the file upload property editor.
    /// </summary>
    internal class FileUploadPropertyValueEditor : DataValueEditor
    {
        private readonly MediaFileManager _mediaFileManager;
        private readonly ContentSettings _contentSettings;

        public FileUploadPropertyValueEditor(
            DataEditorAttribute attribute,
            MediaFileManager mediaFileManager,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IOptions<ContentSettings> contentSettings,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
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
        public override object FromEditor(ContentPropertyData editorValue, object currentValue)
        {
            var currentPath = currentValue as string;
            if (!currentPath.IsNullOrWhiteSpace())
                currentPath = _mediaFileManager.FileSystem.GetRelativePath(currentPath);

            string editorFile = null;
            if (editorValue.Value != null)
            {
                editorFile = editorValue.Value as string;
            }

            // ensure we have the required guids
            var cuid = editorValue.ContentKey;
            if (cuid == Guid.Empty) throw new Exception("Invalid content key.");
            var puid = editorValue.PropertyTypeKey;
            if (puid == Guid.Empty) throw new Exception("Invalid property type key.");

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
                    _mediaFileManager.FileSystem.DeleteFile(currentPath);
                    return null; // clear
                }

                return currentValue; // unchanged
            }

            // process the file
            var filepath = editorFile == null ? null : ProcessFile(editorValue, file, currentPath, cuid, puid);

            // remove all temp files
            foreach (var f in uploads)
                File.Delete(f.TempFilePath);

            // remove current file if replaced
            if (currentPath != filepath && string.IsNullOrWhiteSpace(currentPath) == false)
                _mediaFileManager.FileSystem.DeleteFile(currentPath);

            // update json and return
            if (editorFile == null) return null;
            return filepath == null ? string.Empty : _mediaFileManager.FileSystem.GetUrl(filepath);


        }

        private string ProcessFile(ContentPropertyData editorValue, ContentPropertyFile file, string currentPath, Guid cuid, Guid puid)
        {
            // process the file
            // no file, invalid file, reject change
            if (UploadFileTypeValidator.IsValidFileExtension(file.FileName, _contentSettings) == false)
                return null;

            // get the filepath
            // in case we are using the old path scheme, try to re-use numbers (bah...)
            var filepath = _mediaFileManager.GetMediaPath(file.FileName, currentPath, cuid, puid); // fs-relative path

            using (var filestream = File.OpenRead(file.TempFilePath))
            {
                // TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
                // since we'd need to be able to return values for other properties from these methods

                _mediaFileManager.FileSystem.AddFile(filepath, filestream, true); // must overwrite!
            }

            return filepath;
        }
    }
}
