using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    public class TwoFactorAuthenticationInfo
    {
        public string Secret { get; set; }
        public string Email { get; set; }
        public string ApplicationName { get; set; }
    }
}
