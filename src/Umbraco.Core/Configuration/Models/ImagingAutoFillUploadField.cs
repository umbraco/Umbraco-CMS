using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    public class ImagingAutoFillUploadField : ValidatableEntryBase
    {
        [Required]
        public string Alias { get; set; }

        [Required]
        public string WidthFieldAlias { get; set; }

        [Required]
        public string HeightFieldAlias { get; set; }

        [Required]
        public string LengthFieldAlias { get; set; }

        [Required]
        public string ExtensionFieldAlias { get; set; }
    }
}
