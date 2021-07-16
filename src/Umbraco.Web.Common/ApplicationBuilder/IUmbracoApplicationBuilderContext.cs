using System;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{

    public interface IUmbracoApplicationBuilderContext : IUmbracoApplicationBuilderServices
    {
        Action RegisterDefaultRequiredMiddleware { get; }
    }
}
