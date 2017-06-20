using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "userGroup", Namespace = "")]
    public class UserGroupSave : EntityBasic, IValidatableObject
    {
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public new int Id { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required]
        public override string Alias { get; set; }
        
        [DataMember(Name = "sections")]
        public IEnumerable<string> Sections { get; set; }

        [DataMember(Name = "users")]
        public IEnumerable<int> Users { get; set; }

        [DataMember(Name = "startContentId")]
        public int StartContentId { get; set; }

        [DataMember(Name = "startMediaId")]
        public int StartMediaId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //TODO: Add other server side validation
            //if (CultureInfo.GetCultureInfo(Culture))
            //    yield return new ValidationResult("The culture is invalid", new[] { "Culture" });

            yield break;
        }
    }
}