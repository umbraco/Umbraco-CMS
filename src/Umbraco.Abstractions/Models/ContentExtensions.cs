using System.Linq;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        public static bool IsAnyUserPropertyDirty(this IContentBase entity)
        {
            return entity.Properties.Any(x => x.IsDirty());
        }

        public static bool WasAnyUserPropertyDirty(this IContentBase entity)
        {
            return entity.Properties.Any(x => x.WasDirty());
        }


    }
}
