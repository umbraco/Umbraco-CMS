using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Model used to save a document type
    /// </summary>
    [DataContract(Name = "contentType", Namespace = "")]
    public class DocumentTypeSave : ContentTypeSave<PropertyTypeBasic>
    {   
        /// <summary>
        /// The list of allowed templates to assign (template alias)
        /// </summary>
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<string> AllowedTemplates { get; set; }
        
        /// <summary>
        /// The default template to assign (template alias)
        /// </summary>
        [DataMember(Name = "defaultTemplate")]
        public string DefaultTemplate { get; set; }

        /// <summary>
        /// Custom validation
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AllowedTemplates.Any(x => StringExtensions.IsNullOrWhiteSpace(x)))
                yield return new ValidationResult("Template value cannot be null", new[] { "AllowedTemplates" });

            foreach (var v in base.Validate(validationContext))
            {
                yield return v;
            }
        }
    }
}