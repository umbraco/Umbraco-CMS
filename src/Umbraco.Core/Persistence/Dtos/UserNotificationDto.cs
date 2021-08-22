using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence.Dtos
{
    public class UserNotificationDto
    {
        public int nodeId { get; set; }
        public int userId { get; set; }

        public Guid nodeObjectType { get; set; }
        public string action { get; set; }
    }
}
