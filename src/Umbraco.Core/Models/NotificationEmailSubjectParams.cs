using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Models
{

    public class NotificationEmailSubjectParams
    {
        public NotificationEmailSubjectParams(string siteUrl, string action, string itemName)
        {
            SiteUrl = siteUrl ?? throw new ArgumentNullException(nameof(siteUrl));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
        }

        public string SiteUrl { get; }
        public string Action { get;  }
        public string ItemName { get;  }
    }
}
