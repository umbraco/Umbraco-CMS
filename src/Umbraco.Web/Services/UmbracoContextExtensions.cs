using System.Collections.Concurrent;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Web.Publishing;
using Umbraco.Web.Services;

namespace Umbraco.Web
{
    /// <summary>
    /// Service specific extensions for <see cref="UmbracoContext"/>, which provides access to the various services
    /// </summary>
    public static class UmbracoContextExtensions
    {
        //TODO Add services to a dictionary, so we don't create a new instance each time a service is needed
        private static readonly ConcurrentDictionary<string, IService> ServiceCache = new ConcurrentDictionary<string, IService>();

        public static IContentService ContentService(this UmbracoContext umbracoContext)
        {
            return new ContentService(new PetaPocoUnitOfWorkProvider(), new PublishingStrategy());
        }

        public static IContentTypeService ContentTypeService(this UmbracoContext umbracoContext)
        {
            var contentService = umbracoContext.ContentService();
            return new ContentTypeService(contentService, new PetaPocoUnitOfWorkProvider());
        }
    }
}