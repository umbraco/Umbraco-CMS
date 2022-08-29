using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoTwoFactorLogin
    {
        public int Id { get; set; }
        public Guid UserOrMemberKey { get; set; }
        public string ProviderName { get; set; } = null!;
        public string Secret { get; set; } = null!;
    }
}
