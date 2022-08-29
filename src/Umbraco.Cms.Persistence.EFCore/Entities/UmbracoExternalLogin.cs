using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoExternalLogin
    {
        public UmbracoExternalLogin()
        {
            UmbracoExternalLoginTokens = new HashSet<UmbracoExternalLoginToken>();
        }

        public int Id { get; set; }
        public Guid UserOrMemberKey { get; set; }
        public string LoginProvider { get; set; } = null!;
        public string ProviderKey { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? UserData { get; set; }

        public virtual ICollection<UmbracoExternalLoginToken> UmbracoExternalLoginTokens { get; set; }
    }
}
