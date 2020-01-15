using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    internal class UploadFileTypeValidator : IValueValidator
    {
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoSettingsSection _umbracoSettingsSection;

        public UploadFileTypeValidator(ILocalizedTextService localizedTextService, IUmbracoSettingsSection umbracoSettingsSection)
        {
            _localizedTextService = localizedTextService;
            _umbracoSettingsSection = umbracoSettingsSection;
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
                if (IsValidFileExtension(filename, _umbracoSettingsSection) == false)
                {
                    //we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    yield return new ValidationResult(_localizedTextService.Localize("errors/dissallowedMediaType"), new[] { "value" });
                }
            }
        }

        internal static bool IsValidFileExtension(string fileName, IUmbracoSettingsSection umbracoSettingsSection)
        {
            if (fileName.IndexOf('.') <= 0) return false;
            var extension = new FileInfo(fileName).Extension.TrimStart(".");
            return umbracoSettingsSection.Content.IsFileAllowedForUpload(extension);
        }
    }
}
