using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents the data used to persist a user
    /// </summary>
    /// <remarks>
    /// This will be different from the model used to display a user and we don't want to "Overpost" data back to the server,
    /// and there will most likely be different bits of data required for updating passwords which will be different from the
    /// data used to display vs save
    /// </remarks>
    [DataContract(Name = "user", Namespace = "")]
    public class UserSave : EntityBasic, IValidatableObject
    {
        //TODO: There will be more information to save along with the structure for changing passwords

        [DataMember(Name = "locale", IsRequired = true)]
        [Required]
        public string Culture { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataMember(Name = "userType")]
        [Required]
        public string UserType { get; set; }

        [DataMember(Name = "startContentId")]
        public int StartContentId { get; set; }

        [DataMember(Name = "startMediaId")]
        public int StartMediaId { get; set; }
        
        [DataMember(Name = "allowedSections")]
        public IEnumerable<string> AllowedSections { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //TODO: Add other server side validation
            //if (CultureInfo.GetCultureInfo(Culture))
            //    yield return new ValidationResult("The culture is invalid", new[] { "Culture" });

            yield break;
        }
    }
}