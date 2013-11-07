using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    internal class UploadFileTypeValidator : IPropertyValidator
    {
        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {

            //now check the file type
            var asJson = value as JObject;
            if (asJson == null) yield break;
            if (asJson["selectedFiles"] == null) yield break;
            var fileNames = asJson["selectedFiles"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var fileName in fileNames)
            {
                if (ValidateFileExtension(fileName) == false)
                {
                    yield return new ValidationResult(ui.Text("errors", "dissallowedMediaType"),
                                                      new[]
                                                          {
                                                              //we only store a single value for this editor so the 'member' or 'field' 
                                                              // we'll associate this error with will simply be called 'value'
                                                              "value"
                                                          });   
                }
            }

        }

        internal static bool ValidateFileExtension(string fileName)
        {
            if (fileName.IndexOf('.') <= 0) return false;
            var extension = Path.GetExtension(fileName).TrimStart(".");
            return UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Any(x => StringExtensions.InvariantEquals(x, extension)) == false;
        }

    }
}