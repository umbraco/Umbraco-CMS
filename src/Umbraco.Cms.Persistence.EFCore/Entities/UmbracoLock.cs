using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoLock
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public string Name { get; set; } = null!;
    }
}
