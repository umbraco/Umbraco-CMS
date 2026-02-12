using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Tests.AcceptanceTest.ExternalLogin.AzureADB2C
{
    public class AzureB2CComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<AzureB2COptions>();

            builder.ConfigureAuthentication(builder.Config);
        }
    }
}
