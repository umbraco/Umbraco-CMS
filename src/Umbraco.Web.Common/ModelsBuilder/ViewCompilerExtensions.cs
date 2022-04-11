using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Caching.Memory;

namespace Umbraco.Cms.Web.Common.ModelsBuilder
{
    internal static class ViewCompilerExtensions
    {
        internal static bool Invalidate(this IViewCompiler compiler)
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            if (compiler.GetType().GetField("_cache", bindingFlags)?.GetValue(compiler) is not MemoryCache cache)
            {
                return false;
            }

            cache.Compact(100);
            return true;
        }
    }
}
