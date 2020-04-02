using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Hosting;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.WebAssets;
using Umbraco.Infrastructure.WebAssets;

namespace Umbraco.Web.JavaScript
{
    /// <summary>
    /// Reads from all defined manifests and ensures that any of their initialization is output with the
    /// main Umbraco initialization output.
    /// </summary>
    public class JsInitialization
    {
        
        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetDefaultInitialization()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.JsInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }
    }
}
