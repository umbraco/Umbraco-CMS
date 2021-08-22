using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence.Dtos
{
    public class DefinedIndexDto

    {
        public string TABLE_NAME { get; set; }
        public string INDEX_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public short UNIQUE { get; set; }
    }
}
