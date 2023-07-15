using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.UI
{
    public class TestController : UmbracoApiController
    {
        private readonly IDbContextFactory<UmbracoInternalDbContext> _dbContextFactory;

        public TestController(IDbContextFactory<UmbracoInternalDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [HttpGet]
        public string Hello()
        {
            var db = _dbContextFactory.CreateDbContext();
            var b = db.CmsContentNus.FirstOrDefault();
            return $"Hello {b == null}";
        }
    }
}
