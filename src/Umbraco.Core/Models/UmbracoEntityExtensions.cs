using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    internal static class UmbracoEntityExtensions
    {

        public static object GetAdditionalDataValueIgnoreCase(this IUmbracoEntity entity, string key, object defaultVal)
        {
            if (entity.AdditionalData.ContainsKeyIgnoreCase(key) == false) return defaultVal;
            return entity.AdditionalData.GetValueIgnoreCase(key, defaultVal);
        }

        public static bool IsContainer(this IUmbracoEntity entity)
        {
            if (entity.AdditionalData.ContainsKeyIgnoreCase("IsContainer") == false) return false;
            var val = entity.AdditionalData.GetValueIgnoreCase("IsContainer", null);
            if (val is bool && (bool) val)
            {
                return true;
            }
            return false;
        }

    }
}
