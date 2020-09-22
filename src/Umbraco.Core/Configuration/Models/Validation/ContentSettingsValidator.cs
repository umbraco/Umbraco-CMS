using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class ContentSettingsValidator : ConfigurationValidatorBase, IValidateOptions<ContentSettings>
    {
        public ValidateOptionsResult Validate(string name, ContentSettings options)
        {
            string message;
            if (!ValidateError404Collection(options.Error404Collection, out message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            if (!ValidateAutoFillImageProperties(options.Imaging.AutoFillImageProperties, out message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateError404Collection(IEnumerable<ContentErrorPage> values, out string message)
        {
            return ValidateCollection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.Error404Collection)}", values, "Culture and one and only one of ContentId, ContentKey and ContentXPath must be specified for each entry", out message);
        }

        private bool ValidateAutoFillImageProperties(IEnumerable<ImagingAutoFillUploadField> values, out string message)
        {
            return ValidateCollection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.Imaging)}:{nameof(ContentSettings.Imaging.AutoFillImageProperties)}", values, "Alias, WidthFieldAlias, HeightFieldAlias, LengthFieldAlias and ExtensionFieldAlias must be specified for each entry", out message);
        }
    }
}
