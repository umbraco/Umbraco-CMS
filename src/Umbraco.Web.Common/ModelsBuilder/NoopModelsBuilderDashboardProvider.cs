using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Common.ModelsBuilder
{
    public class NoopModelsBuilderDashboardProvider: IModelsBuilderDashboardProvider
    {
        public string GetUrl() => string.Empty;
    }
}
