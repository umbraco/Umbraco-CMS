using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.Composing;

namespace Umbraco.Web.PropertyEditors
{
    internal class UploadFileTypeValidator : IValueValidator
    {
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            if (!(value is JObject jobject) || jobject["selectedFiles"] == null) yield break;

            var fileNames = jobject["selectedFiles"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var fileName in fileNames)
            {
                if (ValidateFileExtension(fileName) == false)
                {
                    //we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    yield return new ValidationResult(Current.Services.TextService.Localize("errors/dissallowedMediaType"), new[] { "value" });
                }
            }
        }

        internal static bool ValidateFileExtension(string fileName)
        {
            if (fileName.IndexOf('.') <= 0) return false;
            var extension = Path.GetExtension(fileName).TrimStart(".");
            return UmbracoConfig.For.UmbracoSettings().Content.IsFileAllowedForUpload(extension);
        }
    }
}
