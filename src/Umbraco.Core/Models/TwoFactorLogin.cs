using System;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models
{
    public class TwoFactorLogin : EntityBase, ITwoFactorLogin
    {
        public string ProviderName { get; set; }
        public string Secret { get; set; }
        public Guid UserOrMemberKey { get; set; }
        public bool Confirmed { get; set; }
    }
}
