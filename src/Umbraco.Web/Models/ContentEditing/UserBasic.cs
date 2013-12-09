using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A basic structure the represents a user
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    public class UserBasic : System.IComparable
    {
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int UserId { get; set; }
        
        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string Name { get; set; }


        int System.IComparable.CompareTo(object obj)
        {
            return Name.CompareTo(((UserBasic)obj).Name);
       }
    }
}