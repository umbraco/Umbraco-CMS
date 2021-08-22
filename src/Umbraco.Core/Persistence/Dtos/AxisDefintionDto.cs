using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence.Dtos
{
    public class AxisDefintionDto
    {
        public int nodeId { get; set; }
        public string alias { get; set; }
        public int parentID { get; set; }
    }
}
