using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.Composing;

namespace Umbraco.Web.PropertyEditors
{
    internal class UploadFileTypeValidator : IValueValidator
    {
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
                if (IsValidFileExtension(filename) == false)
                {
                    //we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    yield return new ValidationResult(Current.Services.TextService.Localize("errors/dissallowedMediaType"), new[] { "value" });
                }
            }
        }
        
        internal static bool IsValidFileExtension(string fileName)
        {
            if (fileName.IndexOf('.') <= 0) return false;
            var extension = new FileInfo(fileName).Extension.TrimStart(".");
            return Current.Configs.Settings().Content.IsFileAllowedForUpload(extension);
        }
    }
}
