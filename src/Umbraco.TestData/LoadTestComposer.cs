using Umbraco.Core.Composing;
using System.Configuration;
using Umbraco.Web.PublishedCache.NuCache;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData
{
    public class LoadTestComposer : ComponentComposer<LoadTestComponent>, IUserComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return;

            composition.Register(typeof(LoadTestController), Lifetime.Request);

            if (ConfigurationManager.AppSettings["Umbraco.TestData.IgnoreLocalDb"] == "true")
            {
                composition.Register(factory => new PublishedSnapshotServiceOptions
                {
                    IgnoreLocalDb = true
                });
            }   
        }
    }
}
