using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;

namespace Umbraco.Web.Strategies
{
    public class UpdateMultipleContentCache
    {
        public UpdateMultipleContentCache()
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        void PublishingStrategy_Published(object sender, Core.PublishingEventArgs e)
        {
            if ((sender is IEnumerable<IContent>) == false) return;
        }
    }
}