using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{
    public class UserInviteEmail
    {
        public string Name { get; set; }

        public string FromName { get; set; }

        public string Email { get; set; }

        public IEnumerable<UserGroupBasic> UserGroups { get; set; }
        
        public IEnumerable<EntityBasic> StartContentIds { get; set; }
        
        public IEnumerable<EntityBasic> StartMediaIds { get; set; }
        
        public string InviteUrl { get; set; }

        public string Message { get; set; }
    }
}