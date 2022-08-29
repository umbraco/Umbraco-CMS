using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoLogViewerQuery
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Query { get; set; } = null!;
    }
}
