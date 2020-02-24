﻿using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    public class MemberExportModel
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Groups { get; set; }
        public string ContentTypeAlias { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<MemberExportProperty> Properties { get; set; }
    }
}
