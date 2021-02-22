using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.ModelsBuilder
{
    public interface IModelsBuilderDashboardProvider
    {
        string GetUrl();
    }
}
