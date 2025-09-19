using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace My.Site;

public class MySegmentComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // register the custom segment service in place of the Umbraco core implementation
        builder.Services.AddUnique<ISegmentService, MySegmentService>();

        // update segment configuration so segments are enabled (in the client)
        builder.Services.Configure<SegmentSettings>(settings => settings.Enabled = true);
    }
}