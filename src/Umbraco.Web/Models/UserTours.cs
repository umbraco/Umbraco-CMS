using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "userTours", Namespace = "")]
    public class UserTours
    {
        public UserTours(int userId, IEnumerable<UserTourStatus> tours)
        {
            UserId = userId;
            Tours = tours;
        }

        [DataMember(Name = "userId")]
        public int UserId { get; private set; }

        [DataMember(Name = "tours")]
        public IEnumerable<UserTourStatus> Tours { get; private set; }
    }
}