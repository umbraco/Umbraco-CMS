using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace umbraco.interfaces.skinning
{
    public interface IDependencyType
    {
        String Name { get; set; }

        List<Object> Values { get; set; }
        WebControl Editor { get; set; }
    }
}
