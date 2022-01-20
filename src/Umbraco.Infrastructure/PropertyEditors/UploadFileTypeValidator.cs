// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    internal class UploadFileTypeValidator : IValueValidator
    {
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ContentSettings _contentSettings;

        public UploadFileTypeValidator(ILocalizedTextService localizedTextService, IOptions<ContentSettings> contentSettings)
        {
            _localizedTextService = localizedTextService;
            _contentSettings = contentSettings.Value;
        }

        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            string selectedFiles = null;
            if (value is JObject jobject && jobject["selectedFiles"] is JToken jToken)
            {
                selectedFiles = jToken.ToString();
            }
            else if (valueType?.InvariantEquals(ValueTypes.String) == true)
            {
                selectedFiles = value as string;

                if (string.IsNullOrWhiteSpace(selectedFiles))
                    yield break;
            }

            var fileNames = selectedFiles?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

            if (fileNames == null || !fileNames.Any())
                yield break;

            foreach (string filename in fileNames)
            {
                if (IsValidFileExtension(filename, _contentSettings) is false || IsAllowedInDataTypeConfiguration(filename, dataTypeConfiguration) is false)
                {
                    //we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    yield return new ValidationResult(_localizedTextService.Localize("errors", "dissallowedMediaType"), new[] { "value" });
                }
            }
        }

        internal static bool IsValidFileExtension(string fileName, ContentSettings contentSettings)
        {
            if (TryGetFileExtension(fileName, out var extension) is false)
                return false;

            return contentSettings.IsFileAllowedForUpload(extension);
        }

        internal static bool IsAllowedInDataTypeConfiguration(string filename, object dataTypeConfiguration)
        {
            if (TryGetFileExtension(filename, out var extension) is false)
                return false;

            if (dataTypeConfiguration is FileUploadConfiguration fileUploadConfiguration)
            {
                // If FileExtensions is empty and no allowed extensions have been specified, we allow everything.
                // If there are any extensions specified, we need to check that the uploaded extension is one of them.
                return fileUploadConfiguration.FileExtensions.IsCollectionEmpty() ||
                       fileUploadConfiguration.FileExtensions.Any(x => x.Value.InvariantEquals(extension));
            }

            return false;
        }

        internal static bool TryGetFileExtension(string fileName, out string extension)
        {
            extension = null;
            if (fileName.IndexOf('.') <= 0)
                return false;

            extension = fileName.GetFileExtension().TrimStart(".");
            return true;
        }
    }
}
