using System.Web;
using Umbraco.Core.Persistence;

namespace Umbraco.Core
{
    /// <summary>
    /// Default scope context factory
    /// </summary>
    internal class DefaultScopeContextFactory : IScopeContextFactory
    {
        public IScopeContext GetContext()
        {
            return HttpContext.Current == null ? (IScopeContext)new CallContextScope() : new HttpContextScope();
        }
    }
}