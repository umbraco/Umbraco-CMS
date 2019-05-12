using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    /// <summary>
    /// The supported Property Types that can be used to accept user input to resolve Health Checks
    /// </summary>
    public enum ProvidedValuePropertyType
    {
        TextInput,
        ContentPicker
    }

}
