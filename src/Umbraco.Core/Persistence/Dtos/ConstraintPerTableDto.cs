using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence.Dtos
{
    public class ConstraintPerTableDto
    {
        public string TABLE_NAME { get; set; }
        public string CONSTRAINT_NAME { get; set; }
    }
}
