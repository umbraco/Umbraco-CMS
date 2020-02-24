﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "language", Namespace = "")]
    public class Language
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "culture", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string IsoCode { get; set; }

        [DataMember(Name = "name")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [DataMember(Name = "isDefault")]
        public bool IsDefault { get; set; }

        [DataMember(Name = "isMandatory")]
        public bool IsMandatory { get; set; }

        [DataMember(Name = "fallbackLanguageId")]
        public int? FallbackLanguageId { get; set; }
    }
}
