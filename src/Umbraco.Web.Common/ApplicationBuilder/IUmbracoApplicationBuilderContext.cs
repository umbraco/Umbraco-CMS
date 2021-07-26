using System;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{

    public interface IUmbracoApplicationBuilderContext : IUmbracoApplicationBuilderServices
    {
        void UseUmbracoCoreMiddleware();

        void RunPrePipeline();
        void RunPostPipeline();

        void RegisterDefaultRequiredMiddleware();
    }
}
