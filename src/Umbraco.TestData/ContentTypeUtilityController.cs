using System.Linq;
using Umbraco.Core;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Core.Scoping;

namespace Umbraco.TestData
{

    public class ContentTypeUtilityController : SurfaceController
    {
        private readonly IScopeProvider _scopeProvider;

        public ContentTypeUtilityController(IScopeProvider scopeProvider, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger, umbracoHelper)
        {
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Deletes content types for the alias
        /// </summary>
        /// <param name="alias">The alias can be a prefix wildcard like "home*" which will delete
        /// all content types starting with the alias "home"</param>
        /// <returns></returns>
        public ActionResult DeleteContentTypes(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return Content("No alias specified");
            }

            using (var scope = _scopeProvider.CreateScope())
            {
                if (alias.EndsWith("*"))
                {
                    alias = alias.TrimEnd("*");
                    var cts = Services.ContentTypeService.GetAll()
                        .Where(x => x.Alias.StartsWith(alias))
                        .ToList();
                    foreach (var ct in cts)
                    {
                        Services.ContentTypeService.Delete(ct);
                    }
                }
                else
                {
                    var ct = Services.ContentTypeService.Get(alias);
                    if (ct != null)
                    {
                        Services.ContentTypeService.Delete(ct);
                    }
                }

                scope.Complete();
            }

            return Content("Done");
        }
    }
}
