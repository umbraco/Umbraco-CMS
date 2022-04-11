using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Common.ModelsBuilder
{
    internal class InvalidatableRazorViewEngine : RazorViewEngine
    {
        public InvalidatableRazorViewEngine(
            IRazorPageFactoryProvider pageFactory,
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder,
            IOptions<RazorViewEngineOptions> optionsAccessor,
            ILoggerFactory loggerFactory,
            DiagnosticListener diagnosticListener)
            : base(pageFactory, pageActivator, htmlEncoder, optionsAccessor, loggerFactory, diagnosticListener)
        {
        }

        internal bool Invalidate()
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo? cacheSetter = typeof(RazorViewEngine).GetProperty("ViewLookupCache", bindingFlags)?.GetSetMethod(true);

            if (cacheSetter == null)
            {
                return false;
            }

            _ = cacheSetter.Invoke(this, new object[] { new MemoryCache(new MemoryCacheOptions()) });
            return true;
        }
    }
}
