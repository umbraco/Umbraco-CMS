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
    public class TemplateUtilityController : SurfaceController
    {
        private readonly IScopeProvider _scopeProvider;

        public TemplateUtilityController(IScopeProvider scopeProvider, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger, umbracoHelper)
        {
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Deletes templates for the alias
        /// </summary>
        /// <param name="alias">The alias can be a prefix wildcard like "home*" which will delete
        /// all content types starting with the alias "home"</param>
        /// <returns></returns>
        public ActionResult DeleteTemplates(string alias)
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
                    var templates = Services.FileService.GetTemplates()
                        .Where(x => x.Alias.StartsWith(alias))
                        .ToList();
                    foreach (var template in templates)
                    {
                        Services.FileService.DeleteTemplate(template.Alias);
                    }
                }
                else
                {
                    Services.FileService.DeleteTemplate(alias);
                }

                scope.Complete();
            }

            return Content("Done");
        }
    }
}
