using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    public sealed class DocType
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string TypeName { get; set; }

        public List<DocTypeProperty> Properties { get; set; }
        public List<DocTypeAssociation> Associations { get; set; }
    }
}
