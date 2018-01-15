using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    // fixme - this needs to go or be refactored!
    internal static class UmbracoEntityExtensions
    {
        public static object GetAdditionalDataValueIgnoreCase(this IUmbracoEntity entity, string key, object defaultVal)
        {
            if (entity.AdditionalData.ContainsKeyIgnoreCase(key) == false) return defaultVal;
            return entity.AdditionalData.GetValueIgnoreCase(key, defaultVal);
        }
    }
}
