using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets additional data.
        /// </summary>
        public static object GetAdditionalDataValueIgnoreCase(this IHaveAdditionalData entity, string key, object defaultValue)
        {
            if (!entity.HasAdditionalData) return defaultValue;
            if (entity.AdditionalData.ContainsKeyIgnoreCase(key) == false) return defaultValue;
            return entity.AdditionalData.GetValueIgnoreCase(key, defaultValue);
        }
    }
}
