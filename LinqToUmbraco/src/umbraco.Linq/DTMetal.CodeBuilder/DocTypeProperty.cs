using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    public sealed class DocTypeProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool Mandatory { get; set; }
        public string RegularExpression { get; set; }
        public Type DatabaseType { get; set; }
        public Guid ControlId { get; set; }
        public string TypeName { get; set; }
    }
}
