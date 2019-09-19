using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Models.Membership
{
    internal class MemberExportModel
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
