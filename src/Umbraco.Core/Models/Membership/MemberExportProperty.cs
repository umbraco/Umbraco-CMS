﻿using System;

namespace Umbraco.Core.Models.Membership
{
    internal class MemberExportProperty
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
