using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoMediaVersion
    {
        public int Id { get; set; }
        public string? Path { get; set; }

        public virtual UmbracoContentVersion IdNavigation { get; set; } = null!;
    }
}
