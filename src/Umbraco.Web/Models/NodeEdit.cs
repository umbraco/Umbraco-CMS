using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    public class NodeEdit
    {
        public int NodeId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ConnectionId { get; set; }
    }
}
