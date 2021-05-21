using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.PublishedCache;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData
{
    public class LoadTestComposer : ComponentComposer<LoadTestComponent>, IUserComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return;

            builder.Services.AddScoped(typeof(LoadTestController), typeof(LoadTestController));

            if (ConfigurationManager.AppSettings["Umbraco.TestData.IgnoreLocalDb"] == "true")
            {
                builder.Services.AddSingleton(factory => new PublishedSnapshotServiceOptions
                {
                    IgnoreLocalDb = true
                });
            }
        }
    }
}
