using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence.Dtos
{
    public class DefaultConstraintPerColumnDto
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
    }
}
