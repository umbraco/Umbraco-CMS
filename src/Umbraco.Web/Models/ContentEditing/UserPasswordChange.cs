using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    public class UserPasswordChange
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
