using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Core.Embed
{
    public class Result
    {
        public Status Status { get; set; }
        public bool SupportsDimensions { get; set; }
        public string Markup { get; set; }

    }
}