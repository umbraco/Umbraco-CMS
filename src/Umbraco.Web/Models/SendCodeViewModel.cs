using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Used for 2FA verification
    /// </summary>
    [DataContract(Name = "code", Namespace = "")]
    public class Verify2FACodeModel
    {
        [Required]
        [DataMember(Name = "code", IsRequired = true)]
        public string Code { get; set; }

        [Required]
        [DataMember(Name = "provider", IsRequired = true)]
        public string Provider { get; set; }
    }
}