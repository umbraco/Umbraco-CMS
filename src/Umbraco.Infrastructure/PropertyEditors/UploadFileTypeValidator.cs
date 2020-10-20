using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
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

            var fileNames = selectedFiles?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (fileNames == null || !fileNames.Any())
                yield break;

            foreach (string filename in fileNames)
            {
                if (IsValidFileExtension(filename, _contentSettings) == false)
                {
                    //we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    yield return new ValidationResult(_localizedTextService.Localize("errors/dissallowedMediaType"), new[] { "value" });
                }
            }
        }

        internal static bool IsValidFileExtension(string fileName, ContentSettings contentSettings)
        {
            if (fileName.IndexOf('.') <= 0) return false;
            var extension = fileName.GetFileExtension().TrimStart(".");
            return contentSettings.IsFileAllowedForUpload(extension);
        }
    }
}
